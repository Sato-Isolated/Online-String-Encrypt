<?php
// Include the configuration file
include 'config.php';

$secretKey = 'your_secret_key_for_hmac'; // Secret key to sign the token

// Read the POST request body
$input = json_decode(file_get_contents('php://input'), true);

if (!isset($_POST['app_md5'])) {
    http_response_code(400);
    echo "Application MD5 is required.";
    exit;
}

$app_md5 = $_POST['app_md5'];

// Log to verify the received MD5
error_log("MD5 received: $app_md5");
error_log("Expected MD5: $protectedMd5");

// Verify the authenticity of the application
if ($app_md5 !== $protectedMd5) {
    http_response_code(401);
    echo "Unauthorized";
    error_log("Error: Unauthorized");
    exit;
}

// Generate a token based on the unique ID and a timestamp
$timestamp = time();
$data = $app_md5 . '|' . $timestamp;
$token = hash_hmac('sha256', $data, $secretKey);

// Log the token details
error_log("Generated token: $token");
error_log("Timestamp: $timestamp");

echo $token . "|" . $timestamp; // Plain text response
?>
