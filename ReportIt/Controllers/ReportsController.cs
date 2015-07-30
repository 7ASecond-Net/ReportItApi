using ReportIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;

namespace ReportIt.Controllers
{
    public class ReportsController : ApiController
    {

        // POST api/Reports
        public string Post([FromBody]string value)
        {
            // Parts received separated by ,
            // PageUrl          -- The Url of the page being reported [0]
            // SrcUrl           -- The SrcUrl of any Image being reported [1]
            // LinkUrl          -- The Url of any link that is being reported [2]
            // SelectionText    -- Any Text that is being reported [3..n]

            try
            {
                // Get the Reported values
                string pageUrl = GetPageUrl(value);
                string srcUrl = GetSrcUrl(value);
                string linkUrl = GetLinkUrl(value);
                string selectionText = GetSelectionText(value);

                DamoclesEntities de = new DamoclesEntities();
                de.Database.Connection.Open(); // Connect to the Database

                // Generate our tables
                EUReported eur = new EUReported();
                SrcUrl su = new SrcUrl();
                LinkUrl lu = new LinkUrl();
                SelectionText st = new SelectionText();

                GenerateSrcUrlTable(srcUrl, eur, su);
                GenerateLinkUrlTable(linkUrl, eur, lu);
                GenerateSelectionTextTable(selectionText, eur, st);
                GeneratePageUrlTable(pageUrl, eur);

                de.EUReporteds.Add(eur);
                de.SaveChanges();
                de.Database.Connection.Close(); // Close the database connection

                // Tidy up a little
                eur = null;
                su = null;
                lu = null;
                st = null;
                de = null;

                return true.ToString();
            }
            catch (Exception ex)
            {
                return ex.InnerException.ToString();
            }
        }

        private static void GeneratePageUrlTable(string pageUrl, EUReported eur)
        {
            eur.PageUrl = pageUrl;
            eur.PageUrlHash = GetSHA512(pageUrl);
            eur.Processed = false;
            eur.UpdatedOn = DateTime.UtcNow;
        }

        private static void GenerateSelectionTextTable(string selectionText, EUReported eur, SelectionText st)
        {
            if (!string.IsNullOrEmpty(selectionText))
            {
                st.Processed = false;
                st.SelectionText1 = selectionText;
                st.SelectionTextHash = GetSHA512(selectionText);
                st.UpdatedOn = DateTime.UtcNow;
                eur.SelectionTexts.Add(st); // Add the Link Url data to EUReported
            }
        }

        private static void GenerateLinkUrlTable(string linkUrl, EUReported eur, LinkUrl lu)
        {
            if (!string.IsNullOrEmpty(linkUrl))
            {
                lu.Processed = false;
                lu.LinkUrl1 = linkUrl;
                lu.LinkUrlHash = GetSHA512(linkUrl);
                lu.UpdatedOn = DateTime.UtcNow;
                eur.LinkUrls.Add(lu); // Add the Link Url data to EUReported
            }
        }

        private static void GenerateSrcUrlTable(string srcUrl, EUReported eur, SrcUrl su)
        {
            if (!string.IsNullOrEmpty(srcUrl))
            {
                su.Processed = false;
                su.SrcUrl1 = srcUrl;
                su.SrcUrlHash = GetSHA512(srcUrl);
                su.UpdatedOn = DateTime.UtcNow;
                eur.SrcUrls.Add(su); // Add the Source Url data to EUReported
            }
        }


        /// <summary>
        /// Gets the text that has been selected for reporting
        /// </summary>
        /// <param name="value">
        /// string: Comma delimited string sent by the ReportIt browser Extension
        /// </param>
        /// <returns>
        /// string: The selected text
        /// </returns>
        private string GetSelectionText(string value)
        {
            string[] parts = value.Split(',');
            string sText = "";
            int idx = 0;


            foreach (string txt in parts)
            {
                if(idx > 2)
                {
                    sText += txt + ", ";
                }

                idx++;
            }

            sText = sText.Substring(0, sText.Length - 2);

            return sText; // Remove the final trailing comma
        }


        /// <summary>
        /// Gets the Url of the link that is being reported
        /// </summary>
        /// <param name="value">
        /// string: Comma delimited string sent by the ReportIt browser Extension
        /// </param>
        /// <returns>
        /// string: The url of the link (Decoded)
        /// </returns>
        private string GetLinkUrl(string value)
        {
            return HttpUtility.UrlDecode(value.Split(',')[2].ToString());
        }


        /// <summary>
        /// Gets the Image SrcUrl that is being reported
        /// </summary>
        /// <param name="value">
        /// string: Comma delimited string sent by the ReportIt browser Extension
        /// </param>
        /// <returns>
        /// string: The Image Src Url (Decoded)
        /// </returns>
        private string GetSrcUrl(string value)
        {
            return HttpUtility.UrlDecode(value.Split(',')[1].ToString());
        }


        /// <summary>
        /// Gets the PageUrl that contains the content being reported or is being reported itself
        /// </summary>
        /// <param name="value">
        /// string: Comma delimited string sent by the ReportIt browser Extension
        /// </param>
        /// <returns>
        /// string: The Page Url (Decoded)
        /// </returns>
        private string GetPageUrl(string value)
        {
            return HttpUtility.UrlDecode(value.Split(',')[0].ToString());
        }

        /// <summary>
        /// Produce a SHA512 Hash of a string
        /// </summary>
        /// <param name="stringData">
        /// string: The string to be hashed
        /// </param>
        /// <returns>
        /// string: A Hexadecimal string hash
        /// </returns>
        public static string GetSHA512(string stringData)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] message = UE.GetBytes(stringData);
            SHA512Managed hashString = new SHA512Managed();
            string hexNumber = "";
            byte[] hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hexNumber += String.Format("{0:x2}", x);
            }
            return hexNumber;
        }
    }
}
