using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PersonalSiteMVC.Models;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using MailKit.Net.Smtp;
using MimeKit;

namespace PersonalSiteMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Project()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult TeamPage1()
        {
            return View();
        }

        public ActionResult TeamPage2()
        {
            return View();
        }

        public ActionResult Resume()
        {
            return View();
        }

        #region Contact Form

        //1. INSTALL MAILKIT VIA NUGET PACKAGE MANAGER
        //2. ADD using MailKit.Net.Smtp;
        //3. ADD using MimeKit;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactViewModel cvm)
        {
            //When a class has validation attributes, that validation should be checked
            //BEFORE attempting to process any of the data they provided.

            if (!ModelState.IsValid)
            {
                //Send them back to the form. We can pass the object to the view 
                //so the form will contain the original information they provided.

                return View(cvm);
            }

            //Create the format for the message content we will receive from the contact form
            string message = $"You have received a new email from your site's contact form!<br/>" +
                $"Sender: {cvm.Name}<br/>Email: {cvm.Email}<br/>Subject: {cvm.Subject}<br/>" +
                $"Message: {cvm.Message}";

            //MIME - Multipurpose Internet Mail Extensions - Allows email to contain
            //information other than ASCII, including audio, video, images, and HTML

            //Create a MimeMessage object to assist with storing/transporting the email
            //information from the contact form.
            var mm = new MimeMessage();

            //Even though the user is the one attempting to send a message to us, the actual sender 
            //of the email is the email user we set up with our hosting provider.
            //We can access the credentials for this email user from our AppSecretKeys.config file.
            mm.From.Add(new MailboxAddress(ConfigurationManager.AppSettings["EmailUser"].ToString()));

            //The recipient of this email will be our personal email address, also stored in AppSecretKeys.config.
            mm.To.Add(new MailboxAddress(ConfigurationManager.AppSettings["EmailTo"].ToString()));

            //The subject will be the one provided by the user, which we stored in our cvm object.
            mm.Subject = cvm.Subject;

            //The body of the message will be formatted with the string we created above.
            mm.Body = new TextPart("HTML") { Text = message };

            //We can set the priority of the message as "urgent" so it will be flagged in our email client.
            mm.Priority = MessagePriority.Urgent;

            //We can also add the user's provided email address to the list of ReplyTo addresses
            //so our replies can be sent directly to them instead of the email user on our hosting provider.
            mm.ReplyTo.Add(new MailboxAddress(cvm.Email));

            //The using directive will create the SmtpClient object used to send the email.
            //Once all of the code inside of the using directive's scope has been executed,
            //it will close any open connections and dispose of the object for us.
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                //First, try with the below line commented out. If it doesn't work, uncomment the line
                //below and try again.
                //client.SslProtocols = System.Security.Authentication.SslProtocols.None;

                //Connect to the mail server using credentials in our AppSecretKeys.config.
                client.Connect(ConfigurationManager.AppSettings["EmailClient"].ToString());

                //Log in to the mail server using the credentials for our email user.
                client.Authenticate(

                    //Username
                    ConfigurationManager.AppSettings["EmailUser"].ToString(),

                    //Password
                    ConfigurationManager.AppSettings["EmailPass"].ToString()

                    );

                //It's possible the mail server may be down when the user attempts to contact us, 
                //so we can "encapsulate" our code to send the message in a try/catch.
                try
                {
                    client.Send(mm);
                }
                catch (Exception ex)
                {
                    //If there is an issue, we can return the user to the View with their form 
                    //information intact and present an error message.
                    ViewBag.ErrorMessage = $"There was an error processing your request. Please " +
                        $"try again later.<br/>Error Message: {ex.StackTrace}";

                    return View(cvm);
                }

            }

            //If all goes well, return a View that displays a confirmation to the user
            //that their email was sent.

            return View("EmailConfirmation", cvm);
        }

        #endregion

    }
}