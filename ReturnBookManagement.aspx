<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReturnBookManagement.aspx.cs" Inherits="fyp.ReturnBookManagement" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Return Book - Scan User ID</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html5-qrcode/2.3.8/html5-qrcode.min.js"></script>
    <style>
        /* General Reset */
        body, html {
            margin: 0;
            padding: 0;
            font-family: Arial, sans-serif;
            background-color: #f7f8fa;
        }

        form {
            margin: 0 auto;
            padding: 20px;
        }

        /* Header Section */
        header {
            background-color: #4CAF50;
            color: white;
            padding: 15px;
            text-align: center;
        }

        header h1 {
            margin: 0;
            font-size: 24px;
        }

        /* Main Content */
        .container {
            max-width: 800px;
            margin: 20px auto;
            background: white;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            border-radius: 8px;
            overflow: hidden;
        }

        .content {
            padding: 20px;
            text-align: center;
        }

        .status-bar {
            font-size: 16px;
            margin-bottom: 10px;
            padding: 10px;
            background-color: #eaf2ff;
            border-left: 4px solid #4CAF50;
            text-align: left;
        }

        .output-box {
            font-size: 18px;
            font-weight: bold;
            margin: 20px 0;
            padding: 10px;
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            border-radius: 4px;
            display: inline-block;
        }

        .buttons {
            margin-top: 20px;
        }

        .buttons button {
            font-size: 16px;
            padding: 10px 20px;
            margin: 10px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .buttons button#start-scan-button {
            background-color: #4CAF50;
            color: white;
        }

        .buttons button#start-scan-button.stop {
            background-color: #f44336;
            color: white;
        }

        .buttons button#send-button {
            background-color: #008CBA;
            color: white;
        }

        .buttons button#send-button:disabled {
            background-color: #ccc;
            cursor: not-allowed;
        }

        .scanner-container {
            display: none;
            margin: 20px auto;
            text-align: center;
        }

        #reader {
            width: 300px;
            height: 300px;
            margin: auto;
        }

        footer {
            margin-top: 20px;
            text-align: center;
            color: #777;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <header>
            <h1>Library Management System</h1>
        </header>
        <div class="container">
            <div class="content">
                <div class="status-bar" id="status">
                    Status: Click the button below to start or stop scanning your User ID.
                </div>
                <div>
                    <p>Detected User ID:</p>
                    <div class="output-box" id="userid-output">None</div>
                </div>
                <div class="buttons">
                    <button type="button" id="start-scan-button">Start Scanning</button>
                    <button type="button" id="send-button" disabled>Proceed to Return Book</button>
                </div>
                <div class="scanner-container" id="scanner-container">
                    <div id="reader"></div>
                </div>
            </div>
        </div>
        <footer>
            <p>&copy; 2024 Library Management System. All Rights Reserved.</p>
        </footer>
    </form>

    <script>
        let qrScanner;
        let detectedUserID = "";

        function startScanner() {
            const scannerContainer = document.getElementById('scanner-container');
            scannerContainer.style.display = 'block';

            qrScanner = new Html5Qrcode("reader");
            qrScanner.start(
                { facingMode: "environment" }, // Use the back camera
                {
                    fps: 10, // Frames per second
                    qrbox: { width: 250, height: 250 } // Scanning box size
                },
                (decodedText) => {
                    // On successful scan
                    detectedUserID = decodedText;
                    document.getElementById('userid-output').innerText = detectedUserID;
                    document.getElementById('send-button').disabled = false;
                    console.log("Detected User ID:", detectedUserID);
                    stopScanner();
                },
                (errorMessage) => {
                    // On error (optional)
                    console.warn(`QR Code Scan Error: ${errorMessage}`);
                }
            ).catch((err) => {
                console.error("QR Code Scanner Error:", err);
                document.getElementById('status').innerText = "Error starting scanner. Please try again.";
            });
        }

        function stopScanner() {
            if (qrScanner) {
                qrScanner.stop().then(() => {
                    const scannerContainer = document.getElementById('scanner-container');
                    scannerContainer.style.display = 'none';
                }).catch((err) => {
                    console.error("Error stopping scanner:", err);
                });
            }
        }

        document.getElementById('start-scan-button').addEventListener('click', function () {
            const scannerContainer = document.getElementById('scanner-container');
            if (!qrScanner) {
                startScanner();
                this.innerText = 'Stop Scanning';
            } else {
                stopScanner();
                this.innerText = 'Start Scanning';
            }
        });

        document.getElementById('send-button').addEventListener('click', function () {
            if (detectedUserID) {
                // Redirect to another page with the detected User ID
                window.location.href = `ProcessReturn.aspx?userid=${detectedUserID}`;
            }
        });
    </script>
</body>
</html>
