using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using Android.Content;
using Android.Webkit;
using System.Text;
using AndroidX.Core.Content;

namespace Mobile.Activities
{
    [Activity(Label = "Report Viewer")]
    public class ReportViewerActivity : Activity
    {
        private WebView _webView;
        private string _filePath;
        private string _fileName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_report_viewer);

            // Get file path from intent
            _filePath = Intent.GetStringExtra("FilePath");
            _fileName = Intent.GetStringExtra("FileName");

            // Set activity title
            Title = _fileName ?? "Report Viewer";

            // Initialize WebView
            _webView = FindViewById<WebView>(Resource.Id.webView);
            _webView.Settings.JavaScriptEnabled = true;
            _webView.Settings.AllowFileAccess = true;
            _webView.Settings.AllowContentAccess = true;

            // Load the CSV file
            LoadCsvFile();

            // Set up buttons
            Button closeButton = FindViewById<Button>(Resource.Id.closeButton);
            Button shareButton = FindViewById<Button>(Resource.Id.shareButton);

            closeButton.Click += (sender, e) => Finish();
            shareButton.Click += ShareButtonClick;
        }

        private void LoadCsvFile()
        {
            try
            {
                // Read the CSV data
                string csvData = File.ReadAllText(_filePath);

                // Convert CSV to HTML table for better viewing
                string htmlContent = ConvertCsvToHtml(csvData);

                // Load the HTML into the WebView
                _webView.LoadDataWithBaseURL(null, htmlContent, "text/html", "UTF-8", null);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error loading report: {ex.Message}", ToastLength.Long).Show();
            }
        }

        private string ConvertCsvToHtml(string csvData)
        {
            // Create a simple HTML table from CSV data
            StringBuilder html = new StringBuilder();
            html.Append("<html><head>");
            html.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.Append("<style>");
            html.Append("table { border-collapse: collapse; width: 100%; }");
            html.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.Append("tr:nth-child(even) { background-color: #f2f2f2; }");
            html.Append("th { background-color: #4361ee; color: white; }");
            html.Append("</style>");
            html.Append("</head><body>");
            html.Append("<h2>" + _fileName + "</h2>");
            html.Append("<div style='overflow-x:auto;'>");
            html.Append("<table>");

            // Split by lines
            string[] lines = csvData.Split('\n');

            bool isFirstRow = true;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                html.Append("<tr>");

                // Use proper CSV parsing for handling commas inside quoted fields
                List<string> fields = ParseCsvLine(line);

                foreach (string field in fields)
                {
                    if (isFirstRow)
                    {
                        html.AppendFormat("<th>{0}</th>", field);
                    }
                    else
                    {
                        html.AppendFormat("<td>{0}</td>", field);
                    }
                }

                html.Append("</tr>");
                isFirstRow = false;
            }

            html.Append("</table>");
            html.Append("</div>");
            html.Append("</body></html>");

            return html.ToString();
        }

        private List<string> ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            StringBuilder field = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote inside a quoted field
                        field.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // End of field
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            // Add the last field
            fields.Add(field.ToString());

            return fields;
        }

        private void ShareButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Create a file to share
                Java.IO.File fileToShare = new Java.IO.File(_filePath);

                // Get URI from FileProvider
                string authority = Application.Context.PackageName + ".fileprovider";
                Android.Net.Uri uri = FileProvider.GetUriForFile(this, authority, fileToShare);

                // Create a share intent
                Intent shareIntent = new Intent();
                shareIntent.SetAction(Intent.ActionSend);
                shareIntent.PutExtra(Intent.ExtraStream, uri);
                shareIntent.SetType("text/csv");
                shareIntent.SetFlags(ActivityFlags.GrantReadUriPermission);

                // Start sharing
                StartActivity(Intent.CreateChooser(shareIntent, "Share Report"));
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Error sharing report: {ex.Message}", ToastLength.Long).Show();
            }
        }
    }
}