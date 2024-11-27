<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="fyp.Register" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Register Page</title>
    <link href='https://unpkg.com/boxicons@2.1.4/css/boxicons.min.css' rel='stylesheet' />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.6.0/css/all.min.css" />
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600&display=swap');

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
            margin-top: 80px;
            margin-bottom: 80px;
            width: 420px;
            background: transparent;
            border: 2px solid rgba(255, 255, 255, .2);
            backdrop-filter: blur(30px);
            box-shadow: 0 0 10px rgba(0, 0, 0, .2);
            color: #fff;
            border-radius: 10px;
            padding: 30px 40px;
        }

            .wrapper h1 {
                font-size: 36px;
                text-align: center;
                margin-bottom: 30px;
            }

        .input-box {
            position: relative;
            width: 100%;
            height: 50px;
            margin-bottom: 20px;
        }

            .input-box input{
                width: 100%;
                height: 100%;
                background: transparent;
                outline: none;
                border: 1px solid #ffffff;
                border-radius: 40px;
                padding: 15px 45px 15px 20px;
                color: #fff;
            }

            textarea{
                min-height:72px;
                min-width: 337px;
                max-width: 337px;
                max-height: 100px;
                background: transparent;
                outline: none;
                border: 1px solid #ffffff;
                border-radius: 40px;
                padding: 15px 45px 15px 20px;
                color: #fff;
            }

                .input-box input::placeholder, .input-box .asp-textbox::placeholder, textarea::placeholder {
                    color: #fff;
                }

            .input-box i {
                position: absolute;
                right: 20px;
                top: 50%;
                transform: translateY(-50%);
                font-size: 20px;
                color: #fff;
            }

        .wrapper .btn {
            width: 100%;
            height: 45px;
            background: #fff;
            border: none;
            outline: none;
            border-radius: 40px;
            cursor: pointer;
            font-size: 16px;
            color: #333;
            font-weight: 600;
        }

        label {
            font-weight: 600;
            font-size: 18px;
            color: #fff;
            margin-left: 10px;
        }

        .error-message {
            color: #FFA500; /* Change to #FF6666 for light red */
            font-size: 14px;
        }

        #back {
            position: absolute;
            top: 20px;
            left: 30px;
            cursor: pointer;
            font-size: 1.3rem;
            color: #fff;
        }

        input:-webkit-autofill,
        input:-webkit-autofill:focus {
            transition: background-color 0s 600000s, color 0s 600000s !important;
        }

        .education-level-dropdown {
            margin-bottom: 20px;
            width: 100%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
            font-size: 14px;
            background-color: #fff;
            color: #333;
            transition: border-color 0.3s;
        }

            /* Hover effect for dropdown */
            .education-level-dropdown:hover {
                border-color: #5cb85c;
            }

            /* Focus effect for dropdown */
            .education-level-dropdown:focus {
                border-color: #66afe9;
                outline: none;
            }
    </style>
</head>
<body>
    <a href="javascript:void(0);" onclick="history.back()">
        <i class="fa-solid fa-chevron-left" id="back">Back To Login</i>
    </a>
    <div class="wrapper">
        <form id="registerForm" runat="server">
            <h1>Signup</h1>

            <!-- Username Field -->
            <label for="txtUserName">Username</label>
            <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtUserName" ErrorMessage="*Username is required" CssClass="error-message" Display="Dynamic"></asp:RequiredFieldValidator>
            <div class="input-box">
                <asp:TextBox ID="txtUserName" runat="server" placeholder="Enter username" CssClass="field"></asp:TextBox>
                <i class='bx bx-user'></i>
            </div>
            

            <!-- Email Field -->
            <label for="txtEmail">Email Address</label><asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="*Email is required" CssClass="error-message" Display="Dynamic"></asp:RequiredFieldValidator>
            <div class="input-box">
                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="Enter email" CssClass="field"></asp:TextBox>
                <i class='bx bx-envelope'></i>
            </div>
            <asp:RegularExpressionValidator 
                ID="revEmail" 
                runat="server" 
                ControlToValidate="txtEmail"
                ValidationExpression="^[^\s@]+@(student\.tarc\.edu\.my|tarc\.edu\.my)$"
                ErrorMessage="Invalid email format. Use @student.tarc.edu.my for students or @tarc.edu.my for teachers."
                CssClass="error-message" 
                Display="Dynamic" /><br />

            <!-- Phone Number Field -->
            <label for="txtPhone">Phone Number</label>
            <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone" ErrorMessage="*Phone number is required" CssClass="error-message" Display="Dynamic"></asp:RequiredFieldValidator>
            <div class="input-box">
                <asp:TextBox ID="txtPhone" runat="server" TextMode="Phone" placeholder="Enter phone number" CssClass="field"></asp:TextBox>
                <i class='bx bx-phone'></i>
            </div>
            <asp:RegularExpressionValidator ID="revPhone" runat="server" ControlToValidate="txtPhone"
                ValidationExpression="^(01[0-9]{8,9})$" ErrorMessage="Invalid phone number format. Must start with 01 and be 10 or 11 digits."
                CssClass="error-message" Display="Dynamic" /><br />

            <!-- Address Field -->
            <label for="txtAddress">Address</label>
            <asp:RequiredFieldValidator ID="rfvAddress" runat="server" ControlToValidate="txtAddress" ErrorMessage="*Address is required" CssClass="error-message" Display="Dynamic"></asp:RequiredFieldValidator><br />
            <div class="input-box">
                <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" placeholder="Enter your address" CssClass="field"></asp:TextBox>
                <i class='bx bx-map'></i>
            </div>

            <br />
            <!-- Education Level Field -->
            <label class="education-level" id="lblEducationLevel" style="display:none;">Education Level</label>
            <asp:RequiredFieldValidator 
                ID="rfvEducationLevel" 
                runat="server" 
                ControlToValidate="ddlEducationLevel" 
                InitialValue="" 
                ErrorMessage="*Please select an education level" 
                CssClass="error-message" 
                Display="Dynamic" 
                style="display:none;" />
            <br />
            <asp:DropDownList 
                ID="ddlEducationLevel" 
                runat="server" 
                CssClass="education-level-dropdown" 
                style="display:none;">
                <asp:ListItem Text="Diploma" Value="Diploma" />
                <asp:ListItem Text="Degree" Value="Degree" />
                <asp:ListItem Text="Master" Value="Master" />
            </asp:DropDownList>

            <!-- Password Field -->
            <label for="newPass">Password</label>
            <asp:RequiredFieldValidator ID="rfvNewPass" runat="server" ControlToValidate="newPass" ErrorMessage="*Password is required" CssClass="error-message" Display="Dynamic"></asp:RequiredFieldValidator>
            <div class="input-box">
                <asp:TextBox ID="newPass" runat="server" TextMode="Password" placeholder="Enter new password" CssClass="field"></asp:TextBox>
                <i class="fa-solid fa-eye-slash" id="toggleNewPass"></i>
            </div>
            <asp:RegularExpressionValidator ID="revNewPass" runat="server" ControlToValidate="newPass" ValidationExpression=".{6,}" ErrorMessage="Password must be at least 6 characters" CssClass="error-message" Display="Dynamic"></asp:RegularExpressionValidator><br />

            <!-- Confirm Password Field -->
            <label for="reNewPass">Confirm Password</label>
            <asp:RequiredFieldValidator ID="rfvReNewPass" runat="server" ControlToValidate="reNewPass" ErrorMessage="*Please enter your confirm password" CssClass="error-message" Display="Dynamic"></asp:RequiredFieldValidator>
            <div class="input-box">
                <asp:TextBox ID="reNewPass" runat="server" TextMode="Password" placeholder="Confirm password" CssClass="field"></asp:TextBox>
                <i class="fa-solid fa-eye-slash" id="toggleReNewPass"></i>
            </div>
            <asp:CompareValidator ID="cvPassword" runat="server" ControlToValidate="reNewPass" ControlToCompare="newPass" ErrorMessage="Passwords do not match" CssClass="error-message" Display="Dynamic"></asp:CompareValidator><br />

            <asp:Button ID="btnRegis" class="btn" runat="server" Text="Sign Up" OnClick="RegisterButton_Click" />
        </form>
    </div>
</body>

<script>
    document.getElementById("<%= txtEmail.ClientID %>").addEventListener("input", function () {
        const email = this.value.trim();
        const eduLevelLabel = document.getElementById("lblEducationLevel");
        const eduLevelDropdown = document.getElementById("<%= ddlEducationLevel.ClientID %>");
        const eduLevelValidator = document.getElementById("<%= rfvEducationLevel.ClientID %>");

        // Check if email belongs to a student
        if (email.endsWith("@student.tarc.edu.my")) {
            // Show the education level fields
            eduLevelLabel.style.display = "block";
            eduLevelDropdown.style.display = "block";
            eduLevelValidator.style.display = "block";
            eduLevelValidator.enabled = true; // Enable validator for students
        } else {
            // Hide the education level fields
            eduLevelLabel.style.display = "none";
            eduLevelDropdown.style.display = "none";
            eduLevelValidator.style.display = "none";
            eduLevelValidator.enabled = false; // Disable validator for teachers

            // Clear any selected value in the dropdown
            eduLevelDropdown.value = "";
        }
    });

    document.getElementById("toggleNewPass").addEventListener("click", function () {
        const passwordField1 = document.getElementById("<%= newPass.ClientID %>");
        const type = passwordField1.getAttribute("type") === "password" ? "text" : "password";
        passwordField1.setAttribute("type", type);
        this.classList.toggle("fa-eye");
        this.classList.toggle("fa-eye-slash");
    });

    document.getElementById("toggleReNewPass").addEventListener("click", function () {
        const passwordField2 = document.getElementById("<%= reNewPass.ClientID %>");
        const type = passwordField2.getAttribute("type") === "password" ? "text" : "password";
        passwordField2.setAttribute("type", type);
        this.classList.toggle("fa-eye");
        this.classList.toggle("fa-eye-slash");
    });
</script>
</html>
