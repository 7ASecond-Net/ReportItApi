
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
using System.Web.Http.Cors;

namespace ReportIt.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportsController : ApiController
    {

        public string Get()
        {
            return "You have successfully connected to the ReportIt! Api";
        }

        // POST api/Reports
        public void Post([FromBody]string value)
        {
            // Parts received separated by ,
            // PageUrl          -- The Url of the page being reported [0]
            // SrcUrl           -- The SrcUrl of any Image being reported [1]
            // LinkUrl          -- The Url of any link that is being reported [2]
            // SelectionText    -- Any Text that is being reported [3..n]

            try
            {
                if (value != null)
                {
                    // Get the Reported values and Hashes of them
                    string pageUrl = GetPageUrl(value);
                    string pageUrlHash = GetSHA512(pageUrl);
                    string srcUrl = GetSrcUrl(value);
                    string srcUrlHash = null;
                    if (!string.IsNullOrEmpty(srcUrl)) srcUrlHash = GetSHA512(srcUrl);
                    string linkUrl = GetLinkUrl(value);
                    string linkUrlHash = null;
                    if (!string.IsNullOrEmpty(linkUrl)) linkUrlHash = GetSHA512(linkUrl);
                    string selectionText = GetSelectionText(value);
                    string selectionTextHash = null;
                    if (!string.IsNullOrEmpty(selectionText)) selectionTextHash = GetSHA512(selectionText);

                    ReportItEntities rie = new ReportItEntities();
                    rie.Database.Connection.Open(); // Connect to the Database

                    // Generate our tables
                    EUReported eur = new EUReported();
                    SrcUrl su = new SrcUrl();
                    ReportedSrcUrl rsu = new ReportedSrcUrl();
                    LinkUrl lu = new LinkUrl();
                    ReportedLinkUrl rlu = new ReportedLinkUrl();
                    SelectionText st = new SelectionText();
                    ReportedSelectedText rst = new ReportedSelectedText();


                    if (!string.IsNullOrEmpty(pageUrl))
                    {
                        GenerateReportedSrcsTables(pageUrl, pageUrlHash, srcUrl, srcUrlHash, rie, su, rsu);
                        GenerateReportedLinksTables(pageUrl, pageUrlHash, linkUrl, linkUrlHash, rie, lu, rlu);
                        GenerateReportedSelectedTextTables(pageUrl, pageUrlHash, selectionText, selectionTextHash, rie, st, rst);
                    }
                    else
                    {
                        //Just drop out the bottom -no PageUrl == nothing to process
                    }

                    rie.Database.Connection.Close();

                    //// Tidy up a little
                    eur = null;
                    su = null;
                    rsu = null;
                    lu = null;
                    rlu = null;
                    st = null;
                    rst = null;
                    rie = null;

                   // return "Success";
                }
               // return "Nothing received";
            }
            catch (Exception ex)
            {
               // return ex.InnerException.ToString();
            }
        }

        private void GenerateReportedSelectedTextTables(string pageUrl, string pageUrlHash, string selectionText, string selectionTextHash, ReportItEntities rie, SelectionText st, ReportedSelectedText rst)
        {
            if (!string.IsNullOrEmpty(selectionText))
            {
                // Does the pageUrl exist? 
                // Does the srcUrl exist?
                // Do they both exist in the ReportedSrcUrls table? They should be it is nice to have a sanity check
                EUReported puRecord = GetPageUrlRecord(rie, pageUrlHash);
                if (puRecord == null)
                {
                    // The page record has never been created
                    // Create Page Record
                    puRecord = new EUReported();
                    puRecord.Count = 1;
                    puRecord.PageUrl = pageUrl;
                    puRecord.PageUrlHash = pageUrlHash;
                    puRecord.Processed = false;
                    puRecord.CreatedOn = DateTime.UtcNow;
                    puRecord.UpdatedOn = DateTime.UtcNow;
                    rie.EUReporteds.Add(puRecord);
                }
                else
                {
                    // The page record has been created - update count
                    puRecord.Count = puRecord.Count + 1;
                    puRecord.UpdatedOn = DateTime.UtcNow;
                }

                // Ok now check to see if the SrcUrl exists
                st = GetSelectionTextRecord(rie, selectionTextHash);
                if (st == null)
                {
                    // The SrcUrl record has never been created
                    // Create SrcUrl record
                    st = new SelectionText();
                    st.Count = 1;
                    st.Processed = false;
                    st.SelectionText1 = selectionText;
                    st.SelectionTextHash = selectionTextHash;
                    st.CreatedOn = DateTime.UtcNow;
                    st.UpdatedOn = DateTime.UtcNow;
                    rie.SelectionTexts.Add(st);
                }
                else
                {
                    // The srcUrl record has been created - update count
                    st.Count = st.Count + 1;
                    st.UpdatedOn = DateTime.UtcNow;
                }


                // Ok Now the Sanity Check to make sure that the Many to Many table is correctly populated
                if (!ReportedSelectedTextExists(rie, pageUrlHash, selectionTextHash))
                {
                    rst.PageUrlHash = pageUrlHash;
                    rst.SelectedTextHash = selectionTextHash;
                    rie.ReportedSelectedTexts.Add(rst);
                }
                rie.SaveChanges();
            }
        }

        private bool ReportedSelectedTextExists(ReportItEntities rie, string pageUrlHash, string selectionTextHash)
        {
            var res = rie.ReportedSelectedTexts.FirstOrDefault(r => r.PageUrlHash == pageUrlHash && r.SelectedTextHash == selectionTextHash);
            if (res == null)
                return false;
            else
                return true;
        }

        private SelectionText GetSelectionTextRecord(ReportItEntities rie, string selectionTextHash)
        {
            return rie.SelectionTexts.FirstOrDefault(r => r.SelectionTextHash == selectionTextHash);
        }

        private void GenerateReportedLinksTables(string pageUrl, string pageUrlHash, string linkUrl, string linkUrlHash, ReportItEntities rie, LinkUrl lu, ReportedLinkUrl rlu)
        {
            if (!string.IsNullOrEmpty(linkUrl))
            {
                // Does the pageUrl exist? 
                // Does the srcUrl exist?
                // Do they both exist in the ReportedSrcUrls table? They should be it is nice to have a sanity check
                EUReported puRecord = GetPageUrlRecord(rie, pageUrlHash);
                if (puRecord == null)
                {
                    // The page record has never been created
                    // Create Page Record
                    puRecord = new EUReported();
                    puRecord.Count = 1;
                    puRecord.PageUrl = pageUrl;
                    puRecord.PageUrlHash = pageUrlHash;
                    puRecord.Processed = false;
                    puRecord.CreatedOn = DateTime.UtcNow;
                    puRecord.UpdatedOn = DateTime.UtcNow;
                    rie.EUReporteds.Add(puRecord);
                }
                else
                {
                    // The page record has been created - update count
                    puRecord.Count = puRecord.Count + 1;
                    puRecord.UpdatedOn = DateTime.UtcNow;
                }

                // Ok now check to see if the SrcUrl exists
                lu = GetLinkUrlRecord(rie, linkUrlHash);
                if (lu == null)
                {
                    // The SrcUrl record has never been created
                    // Create SrcUrl record
                    lu = new LinkUrl();
                    lu.Count = 1;
                    lu.Processed = false;
                    lu.LinkUrl1 = linkUrl;
                    lu.LinkUrlHash = linkUrlHash;
                    lu.CreatedOn = DateTime.UtcNow;
                    lu.UpdatedOn = DateTime.UtcNow;
                    rie.LinkUrls.Add(lu);
                }
                else
                {
                    // The srcUrl record has been created - update count
                    lu.Count = lu.Count + 1;
                    lu.UpdatedOn = DateTime.UtcNow;
                }


                // Ok Now the Sanity Check to make sure that the Many to Many table is correctly populated
                if (!ReportedLinkUrlsExists(rie, pageUrlHash, linkUrlHash))
                {
                    rlu.PageUrlHash = pageUrlHash;
                    rlu.LinkUrlHash = linkUrlHash;
                    rie.ReportedLinkUrls.Add(rlu);
                }
                rie.SaveChanges();
            }
        }

        private LinkUrl GetLinkUrlRecord(ReportItEntities rie, string linkUrlHash)
        {
            return rie.LinkUrls.FirstOrDefault(r => r.LinkUrlHash == linkUrlHash);
        }

        private bool ReportedLinkUrlsExists(ReportItEntities rie, string pageUrlHash, string linkUrlHash)
        {
            var res = rie.ReportedLinkUrls.FirstOrDefault(r => r.PageUrlHash == pageUrlHash && r.LinkUrlHash == linkUrlHash);
            if (res == null)
                return false;
            else
                return true;
        }

        private void GenerateReportedSrcsTables(string pageUrl, string pageUrlHash, string srcUrl, string srcUrlHash, ReportItEntities rie, SrcUrl su, ReportedSrcUrl rsu)
        {
            if (!string.IsNullOrEmpty(srcUrl))
            {
                // Does the pageUrl exist? 
                // Does the srcUrl exist?
                // Do they both exist in the ReportedSrcUrls table? They should be it is nice to have a sanity check
                EUReported puRecord = GetPageUrlRecord(rie, pageUrlHash);
                if (puRecord == null)
                {
                    // The page record has never been created
                    // Create Page Record
                    puRecord = new EUReported();
                    puRecord.Count = 1;
                    puRecord.PageUrl = pageUrl;
                    puRecord.PageUrlHash = pageUrlHash;
                    puRecord.Processed = false;
                    puRecord.CreatedOn = DateTime.UtcNow;
                    puRecord.UpdatedOn = DateTime.UtcNow;
                    rie.EUReporteds.Add(puRecord);
                }
                else
                {
                    // The page record has been created - update count
                    puRecord.Count = puRecord.Count + 1;
                    puRecord.UpdatedOn = DateTime.UtcNow;
                }

                // Ok now check to see if the SrcUrl exists
                su = GetSourceUrlRecord(rie, srcUrlHash);
                if (su == null)
                {
                    // The SrcUrl record has never been created
                    // Create SrcUrl record
                    su = new SrcUrl();
                    su.Count = 1;
                    su.Processed = false;
                    su.SrcUrl1 = srcUrl;
                    su.SrcUrlHash = srcUrlHash;
                    su.CreatedOn = DateTime.UtcNow;
                    su.UpdatedOn = DateTime.UtcNow;
                    rie.SrcUrls.Add(su);
                }
                else
                {
                    // The srcUrl record has been created - update count
                    su.Count = su.Count + 1;
                    su.UpdatedOn = DateTime.UtcNow;
                }


                // Ok Now the Sanity Check to make sure that the Many to Many table is correctly populated
                if (!ReportedSrcUrlsExists(rie, pageUrlHash, srcUrlHash))
                {
                    rsu.PageUrlHash = pageUrlHash;
                    rsu.SrcUrlHash = srcUrlHash;
                    rie.ReportedSrcUrls.Add(rsu);
                }
                rie.SaveChanges();
            }

        }

        private bool ReportedSrcUrlsExists(ReportItEntities rie, string pageUrlHash, string srcUrlHash)
        {
            var res = rie.ReportedSrcUrls.FirstOrDefault(r => r.PageUrlHash == pageUrlHash && r.SrcUrlHash == srcUrlHash);
            if (res == null)
                return false;
            else
                return true;
        }

        private SrcUrl GetSourceUrlRecord(ReportItEntities rie, string srcUrlHash)
        {
            return rie.SrcUrls.FirstOrDefault(r => r.SrcUrlHash == srcUrlHash);
        }

        private EUReported GetPageUrlRecord(ReportItEntities rie, string pageUrlHash)
        {
            return rie.EUReporteds.FirstOrDefault(r => r.PageUrlHash == pageUrlHash);
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
                if (idx > 2)
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
