<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="fyp.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login Page</title>
    <link href='https://unpkg.com/boxicons@2.1.4/css/boxicons.min.css' rel='stylesheet' />
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Noto+Serif+Khitan+Small+Script&family=Poppins:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,100;1,200;1,300;1,400;1,500;1,600;1,700;1,800;1,900&display=swap');

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: "Poppins", sans-serif;
        }

        body {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            background: url("images/login.jpeg") no-repeat;
            background-size: cover;
            background-position: center;
        }

        .wrapper {
            width: 420px;
            background: transparent;
            border: 2px solid rgba(255, 255, 255, .2);
            backdrop-filter: blur(20px);
            box-shadow: 0 0 10px rgba(0, 0, 0, .2);
            color: #fff;
            border-radius: 10px;
            padding: 30px 40px;
        }

            .wrapper h1 {
                font-size: 36px;
                text-align: center;
            }

            .wrapper .input-box {
                position: relative;
                width: 100%;
                height: 50px;
                margin: 30px 0;
            }

        .input-box input, .input-box .asp-textbox {
            width: 100%;
            height: 100%;
            background: transparent;
            border: none;
            outline: none;
            border-radius: 40px;
            font-size: 16px;
            color: #fff;
            padding: 20px 45px 20px 20px;
        }

        .input-box input::placeholder, .input-box .asp-textbox::placeholder {
             color: #fff;
        }

        .input-box i {
            position: absolute;
            right: 20px;
            top: 50%;
            transform: translateY(-50%);
            font-size: 20px;
        }

        .wrapper .remember-forgot {
            display: flex;
            justify-content: space-between;
            font-size: 14.5px;
            margin: -15px 0 15px;
        }

        .wrapper .remember-forgot label {
           color: #fff;
        }

        .wrapper .remember-forgot label input[type="checkbox"] {
            margin-right: 3px;
        }

        .remember-forgot a {
            color: #fff;
            text-decoration: none;
        }

        .remember-forgot a:hover {
             text-decoration: underline;
        }

        .wrapper .btn {
            width: 100%;
            height: 45px;
            background: #fff;
            border: none;
            outline: none;
            border-radius: 40px;
            box-shadow: 0 0 10px rgba(0, 0, 0, .1);
            cursor: pointer;
            font-size: 16px;
            color: #333;
            font-weight: 600;
        }

        .register-link p a {
            color: #fff;
            text-decoration: none;
            font-weight: 600;
        }

        .register-link p a:hover {
            text-decoration: underline;
        }

        input:-webkit-autofill,
        input:-webkit-autofill:focus {
            transition: background-color 0s 600000s, color 0s 600000s !important;
        }
    </style>
</head>
<body>
    <div class="wrapper">
        <form id="loginForm" runat="server">
            <h1>Login</h1>
            <div class="input-box">
                <asp:TextBox placeholder="Username" ID="txtUserName" runat="server" BorderColor="#FFFFFF" BorderStyle="Solid" BorderWidth="2px"></asp:TextBox>
                <i class='bx bx-user'></i>
            </div>
            <div class="input-box">
                <asp:TextBox placeholder="Password" ID="txtPass" runat="server" BorderColor="#FFFFFF" BorderStyle="Solid" BorderWidth="2px" TextMode="Password"></asp:TextBox>

                <i class='bx bxs-lock-alt'></i>
            </div>

            <div class="remember-forgot">
                <label><asp:CheckBox ID="chbRemember" runat="server" />Remember Me</label>
                <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/ForgetPassword.aspx">Forgot Password?</asp:HyperLink>
            </div>
            <asp:Button ID="btnLogin" class="btn" runat="server" Text="Login" OnClick="btnLogin_Click" />
             <div class="register-link">
                <p>Don't have an account?
                    <asp:HyperLink ID="hlReg" runat="server" Text="Register" NavigateUrl="~/Register.aspx"/>
            </div> 

            <asp:Label ID="MessageBox" runat="server" ForeColor="Red"></asp:Label>
        </form>
    </div>
</body>
</html>
<script type="text/javascript">
    // Check if unlockTime is set (from the server-side script)
    if (typeof unlockTime !== 'undefined') {
        // Initialize countdown
        const countdownElement = document.getElementById("MessageBox");
        
        function updateCountdown() {
            const now = new Date();
            const timeRemaining = unlockTime - now;

            if (timeRemaining <= 0) {
                countdownElement.innerText = "Your account has been unlocked.";
                clearInterval(countdownInterval);  // Stop the timer
            } else {
                const minutes = Math.floor((timeRemaining % (1000 * 60 * 60)) / (1000 * 60));
                const seconds = Math.floor((timeRemaining % (1000 * 60)) / 1000);
                countdownElement.innerText = "Your account has been locked for 5 minutes. It will automatically unlock in " +
                                              `${minutes} minutes and ${seconds} seconds.`;
            }
        }

        // Update countdown every second
        const countdownInterval = setInterval(updateCountdown, 1000);
    }
</script>
