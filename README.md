<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>String Encoder - README</title>

</head>
<body>

<h1>String Encoder</h1>

<h2>Overview</h2>
<p>The <strong>String Encoder</strong> is a C# tool designed to encrypt strings within an executable file, store these encrypted strings in a MySQL database, and generate a protected version of the executable along with a corresponding PHP configuration file.</p>

<h2>Features</h2>
<ul>
    <li><strong>String Encryption</strong>: Encrypts all strings in the target executable file and stores them in a MySQL database.</li>
    <li><strong>Database Setup</strong>: Automatically creates and configures a MySQL database and table to store encrypted strings.</li>
    <li><strong>Protected Executable</strong>: Generates a new version of the executable where strings are replaced with calls to a decoding function.</li>
    <li><strong>MD5 Calculation</strong>: Computes the MD5 hash of the protected executable for verification purposes.</li>
    <li><strong>PHP Configuration File Generation</strong>: Creates a PHP config file with database credentials and the MD5 hash for integration with a web service.</li>
</ul>

<h2>Prerequisites</h2>
<p>Before using this tool, ensure you have the following installed:</p>
<ul>
    <li><a href="https://dev.mysql.com/downloads/mysql/">MySQL Server</a></li>
    <li>PHP (for running the PHP configuration and token generation scripts)</li>
</ul>

<h2>Usage</h2>
<ol>
    <li><strong>Database Setup</strong>:
        <p>The tool will create the necessary database and tables if they do not already exist. Ensure your MySQL server is running and accessible.</p>
    </li>
    <li><strong>Encrypting Strings</strong>:
        <p>The tool will replace strings in your executable with encrypted references. These references will be dynamically decrypted during runtime using the <code>Runtime</code> class injected into the executable.</p>
    </li>
    <li><strong>PHP Configuration</strong>:
        <p>The <code>config.php</code> file will contain the necessary credentials and MD5 hash for your web service to validate and interact with the protected executable.</p>
    </li>
</ol>

<h2>Important Notes</h2>
<ul>
    <li><strong>Security</strong>: Ensure your database and PHP scripts are secure and accessible only to authorized users.</li>
</ul>

<h2>Contributing</h2>
<p>Contributions are welcome! Please fork the repository and submit a pull request with your changes. For major changes, please open an issue first to discuss what you would like to change.</p>

<h3 align="left">☕ Support</h3>
<p align="left">If you'd like to support me, you can do so via Ko-fi. Every bit of support is greatly appreciated!</p>
<p align="left">
  <a href='https://ko-fi.com/K3K611OMU5' target='_blank'>
    <img src='https://ko-fi.com/img/githubbutton_sm.svg' alt='Ko-fi' />
  </a>
</p>

<p align="left">You can also support me with cryptocurrency:</p>
<ul align="left">
  <li><strong>XMR :</strong> 48JRJwsDuMQ7EboCSDSAEMKWfyVGWbfBcM5SaxCCMqiBeduwZDZQMw5KseCn2ciyQX6ckJyPH24HJNoJGVZH9EmATAoX6Jz</li>
  <li><strong>LTC :</strong> LVu6dmsaAfp9mi5s6BRFZApBrScQvhYF9s</li>
  <li><strong>BTC :</strong> bc1qps0wd0hhhkz6p924c76s6xc8xt5hn4ctnqtjk2</li>
</ul>

</body>
</html>
