﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InfLauncher.Controllers;
using InfLauncher.Models;

namespace InfLauncher.Views
{
    public partial class NewAccountForm : Form
    {
        private MainController _controller = new MainController();

        public NewAccountForm()
        {
            InitializeComponent();
        }

        public NewAccountForm(MainController controller)
        {
            InitializeComponent();

            _controller = controller;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            var username = txtboxUsername.Text;
            var password = txtboxPassword.Text;
            var email = txtboxEmail.Text;

            _controller.RegisterAccount(new Account.AccountRegistrationRequestModel(username, password, email));
        }
    }
}
