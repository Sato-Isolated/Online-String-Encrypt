<?php
// Include the configuration file
include 'config.php';

$secretKey = 'your_secret_key_for_hmac'; // Same secret key to verify the token

// Read the POST request body
$input = json_decode(file_get_contents('php://input'), true);

if (!isset($_POST['token']) || !isset($_POST['app_md5']) || !isset($_POST['timestamp']) || !isset($_POST['id'])) {
    http_response_code(400);
    echo "Token, MD5, timestamp, and ID are required.";
    exit;
}

// Generate a unique identifier for each log
$logId = uniqid('log_', true);
error_log("=== Start of processing [ID: $logId] ===");

// Extract POST data
$tokenWithTimestamp = trim($_POST['token']);
$app_md5 = $_POST['app_md5'];
$timestamp = $_POST['timestamp'];
$id = filter_var($_POST['id'], FILTER_VALIDATE_INT);

// Log the received values
error_log("[ID: $logId] Token received before separation: $tokenWithTimestamp");
error_log("[ID: $logId] MD5 received: $app_md5");
error_log("[ID: $logId] Timestamp received: $timestamp");
error_log("[ID: $logId] ID received: $id");

// Separate the token and timestamp
list($token, $receivedTimestamp) = explode('|', $tokenWithTimestamp);

// Log after separation
error_log("[ID: $logId] Token after separation: $token");
error_log("[ID: $logId] Timestamp after separation: $receivedTimestamp");

// Verify the application's MD5
if ($app_md5 !== $protectedMd5) {
    http_response_code(401);
    echo "Unauthorized";
    error_log("[ID: $logId] Error: Unauthorized - MD5 does not match");
    error_log("=== End of processing with error [ID: $logId] ===");
    exit;
}

// Recreate the server-side token to verify integrity
$data = $app_md5 . '|' . $timestamp;
$expectedToken = hash_hmac('sha256', $data, $secretKey);

// Log the expected token
error_log("[ID: $logId] Expected token: $expectedToken");

// Compare tokens
if (!hash_equals($expectedToken, $token)) {
    http_response_code(401);
    echo "Invalid token.";
    error_log("[ID: $logId] Error: Unauthorized - Tokens do not match");
    error_log("=== End of processing with error [ID: $logId] ===");
    exit;
}

// Connect to the database with MySQLi
$conn = new mysqli('p:' . $host, $user, $pass, $db);

// Check the connection
if ($conn->connect_error) {
    http_response_code(500);
    echo "Error: " . $conn->connect_error;
    error_log("[ID: $logId] Database connection error: " . $conn->connect_error);
    error_log("=== End of processing with error [ID: $logId] ===");
    exit;
}

// Prepare and execute the MySQLi query
$stmt = $conn->prepare("SELECT encrypted_string FROM EncryptedStrings WHERE id = ?");
$stmt->bind_param("i", $id);
$stmt->execute();
$result = $stmt->get_result();

if ($row = $result->fetch_assoc()) {
    $hex = $row['encrypted_string'];
    error_log("[ID: $logId] Encrypted string found: $hex");
    
    echo decryptAndClean($hex);
} else {
    http_response_code(404);
    echo "No record found.";
    error_log("[ID: $logId] Error: No data found for ID: $id");
    error_log("=== End of processing with error [ID: $logId] ===");
}

// Close the connection
$stmt->close();
$conn->close();

// Successful processing end
error_log("=== Successful processing end [ID: $logId] ===");

function decryptAndClean($encryptedString)
{
    // Decrypt the string (replace this function with your actual decryption function)
    $decryptedString = ConvertHexToUtf8String($encryptedString);

    // Clean the decrypted string
    $cleanedString = cleanDecryptedString($decryptedString);

    return $cleanedString;
}


function cleanDecryptedString($decryptedString)
{
    // Remove newline characters
    $cleanedString = str_replace(array("\r", "\n"), '', $decryptedString);
    
    return $cleanedString;
}

function ConvertHexToUtf8String($hexString)
{
    // Convert the hexadecimal string to Base64
    $base64String = '';
    for ($i = 0; $i < strlen($hexString); $i += 2) {
        $base64String .= chr(hexdec(substr($hexString, $i, 2)));
    }

    // Decode the Base64 string to UTF-8
    $utf8String = base64_decode($base64String);

    return $utf8String;
}
?>
