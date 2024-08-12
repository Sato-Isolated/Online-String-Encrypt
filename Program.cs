using dnlib.DotNet.Emit;
using dnlib.DotNet;
using MySql.Data.MySqlClient;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet.Writer;
using System.Security.Cryptography;

namespace Online_Encrypt_String
{
    internal class Program
    {
        public static MethodDef? Init;

        private static void Main(string[] args)
        {
            // Main menu
            AnsiConsole.Write(new FigletText("String Encoder")
                .Centered()
                .Color(Color.Green));

            // Get the executable path
            var executablePath = AnsiConsole.Ask<string>("Enter the [green]path to the executable[/]:");

            // Set up the database
            var host = AnsiConsole.Ask<string>("Enter the [green]host[/]:");
            var dbName = AnsiConsole.Ask<string>("Enter the [green]database name[/]:");
            var user = AnsiConsole.Ask<string>("Enter the [green]user[/]:");
            var password = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]password[/] (optional):")
                .AllowEmpty());

            string connectionString = $"Server={host};Database={dbName};User={user};Password={password};";

            string savePath = Path.Combine(Path.GetDirectoryName(executablePath),
                Path.GetFileNameWithoutExtension(executablePath) + "_protected" +
                Path.GetExtension(executablePath));

            SetupDatabase(connectionString, dbName);
            var module = ModuleDefMD.Load(executablePath);
            Execute(module, connectionString);

            ModuleWriterOptions opts = new ModuleWriterOptions(module);
            opts.Logger = DummyLogger.NoThrowInstance;
            module.Write(savePath, opts);

            // Calculate the MD5 hash of the protected file
            string protectedMd5 = CalculateMd5(savePath);
            AnsiConsole.MarkupLine($"[green]MD5 of {savePath}: {protectedMd5}[/]");

            // Generate the PHP configuration file
            GeneratePhpConfigFile(host, dbName, user, password, protectedMd5);

            AnsiConsole.MarkupLine("[green]PHP configuration file generated successfully![/]");
        }

        public static void GeneratePhpConfigFile(string host, string dbName, string user, string password, string protectedMd5)
        {
            string configContent = $@"
<?php
$host = '{host}';
$db = '{dbName}';
$user = '{user}';
$pass = '{password}';
$protectedMd5 = '{protectedMd5}';
?>";

            string configPath = Path.Combine(Environment.CurrentDirectory, "config.php");
            File.WriteAllText(configPath, configContent);
        }

        public static string CalculateMd5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static void SetupDatabase(string connectionString, string dbName)
        {
            // Initial connection without specifying the database
            var connectionStringWithoutDb = connectionString.Replace($"Database={dbName};", "");

            using (var connection = new MySqlConnection(connectionStringWithoutDb))
            {
                connection.Open();

                // Create the database if it does not exist
                var createDbCommand = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS {dbName};", connection);
                createDbCommand.ExecuteNonQuery();

                AnsiConsole.MarkupLine("[green]Database created successfully![/]");
            }

            // Now that the database exists, reconnect with the specified database
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var createTableCommand = new MySqlCommand(
                    @"CREATE TABLE IF NOT EXISTS EncryptedStrings (
              id INT PRIMARY KEY AUTO_INCREMENT,
              encrypted_string TEXT NOT NULL
          );", connection);
                createTableCommand.ExecuteNonQuery();

                AnsiConsole.MarkupLine("[green]Tables set up successfully![/]");
            }
        }

        private static int InsertEncryptedString(MySqlConnection connection, string encryptedString)
        {
            var command = new MySqlCommand("INSERT INTO EncryptedStrings (encrypted_string) VALUES (@encrypted); SELECT LAST_INSERT_ID();", connection);
            command.Parameters.AddWithValue("@encrypted", encryptedString);
            int id = Convert.ToInt32(command.ExecuteScalar());
            return id;
        }

        public static void Execute(ModuleDef module, string connectionString)
        {
            InjectClass1(module);
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                if (type.Namespace.Contains("Properties")) continue;
                foreach (var meth in type.Methods)
                {
                    if (!meth.HasBody || !meth.Body.HasInstructions) continue;
                    if (meth.FullName.Contains("<Module>")) continue;
                    for (var i = 0; i < meth.Body.Instructions.Count; i++)
                    {
                        if (meth.Body.Instructions[i].OpCode != OpCodes.Ldstr) continue;

                        string plainText = meth.Body.Instructions[i].Operand.ToString();
                        string encrypted = ConvertStringToUtf8Base64Hex(plainText);

                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            int id = InsertEncryptedString(connection, encrypted);

                            // Replace the current instruction with the ID and a call to the Init method
                            meth.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                            meth.Body.Instructions[i].Operand = id;
                            meth.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Call, Init));
                        }
                    }
                    meth.Body.SimplifyBranches();
                }
            }
        }

        private static string ConvertStringToUtf8Base64Hex(string utf8String)
        {
            // Convert the string to UTF-8 and then to Base64
            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(utf8String));

            // Convert the Base64 string to hexadecimal
            var hexBuilder = new StringBuilder(base64String.Length * 2);
            foreach (var c in base64String)
            {
                hexBuilder.AppendFormat("{0:x2}", (int)c);
            }

            return hexBuilder.ToString();
        }

        private static void InjectClass1(ModuleDef module)
        {
            var typeModule = ModuleDefMD.Load(typeof(Runtime).Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(Runtime).MetadataToken));
            var members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            Init = (MethodDef)members.Single(method => method.Name == "RunDecoder");
            foreach (var md in module.GlobalType.Methods)
            {
                if (md.Name != ".ctor") continue;
                module.GlobalType.Remove(md);
                break;
            }
        }
    }
}
