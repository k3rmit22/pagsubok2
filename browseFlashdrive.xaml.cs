    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Collections.ObjectModel;
    using iTextSharp.text.pdf;
    using iTextSharp.text.pdf.parser;
    using System.Text;

    namespace kiosk_snapprint
    {
        public partial class browseFlashdrive : UserControl
        {
            // ObservableCollection to bind the ListView
            public ObservableCollection<FileItem> PdfFiles { get; set; }

            public browseFlashdrive()
            {
                InitializeComponent();

                // Initialize the collection for the ListView binding
                PdfFiles = new ObservableCollection<FileItem>();
                pdfFileListView.ItemsSource = PdfFiles;

                // Get the flash drive path and populate the PDF files
                PopulatePdfFiles();
            }

            private void PopulatePdfFiles()
            {
                try
                {
                    // Find the removable drive (flash drive) by checking for connected drives
                    var flashDrive = DriveInfo.GetDrives()
                                              .FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Removable);

                    if (flashDrive != null)
                    {
                        string flashDrivePath = flashDrive.RootDirectory.FullName;

                        // Get all PDF files in the root of the flash drive
                        var pdfFiles = Directory.GetFiles(flashDrivePath, "*.pdf", SearchOption.TopDirectoryOnly)
                                                 .Select(filePath => GetPdfDetails(filePath))
                                                 .Where(fileItem => fileItem != null)
                                                 .ToList();

                        // Populate the ListView with the PDF files
                        PdfFiles.Clear();
                        foreach (var pdfFile in pdfFiles)
                        {
                            PdfFiles.Add(pdfFile);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No removable flash drive detected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors that occur during file access
                    MessageBox.Show($"Error accessing flash drive: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private FileItem GetPdfDetails(string filePath)
            {
                try
                {
                    using (var reader = new PdfReader(filePath))
                    {
                        int pageCount = reader.NumberOfPages;
                        var pageSize = reader.GetPageSize(1); // Get size of the first page

                        // Determine paper size
                        string paperSize = "Unknown";
                        if (Math.Abs(pageSize.Width - 595) < 10 && Math.Abs(pageSize.Height - 842) < 10)
                            paperSize = "A4";
                        else if (Math.Abs(pageSize.Width - 612) < 10 && Math.Abs(pageSize.Height - 792) < 10)
                            paperSize = "Letter (Short)";
                        else if (Math.Abs(pageSize.Width - 612) < 10 && Math.Abs(pageSize.Height - 1008) < 10)
                            paperSize = "Legal (Long)";

                        // Check if the PDF contains color or is grayscale
                        string colorStatus = DetectColorStatus(reader);

                        return new FileItem
                        {
                            FileName = System.IO.Path.GetFileName(filePath),
                            FilePath = filePath,
                            PageCount = pageCount,
                            PaperSize = paperSize,
                            ColorStatus = colorStatus
                        };
                    }
                }
                catch
                {
                    return null; // Skip files that cannot be processed
                }
            }

            private string DetectColorStatus(PdfReader reader)
            {
                try
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        var page = reader.GetPageN(i);
                        var resources = page.GetAsDict(PdfName.RESOURCES);

                        // Check for color space in images or content stream
                        if (resources != null)
                        {
                            var xObject = resources.GetAsDict(PdfName.XOBJECT);
                            if (xObject != null)
                            {
                                foreach (var key in xObject.Keys)
                                {
                                    var obj = PdfReader.GetPdfObject(xObject.Get(key));
                                    if (obj.IsStream())
                                    {
                                        var stream = (PRStream)obj;
                                        var subtype = stream.GetAsName(PdfName.SUBTYPE);

                                        // Check if the object is an image
                                        if (PdfName.IMAGE.Equals(subtype))
                                        {
                                            var colorSpace = stream.GetAsName(PdfName.COLORSPACE);
                                            if (colorSpace != null)
                                            {
                                                if (PdfName.DEVICERGB.Equals(colorSpace) || PdfName.DEVICECMYK.Equals(colorSpace))
                                                {
                                                    return "Colored"; // Return Colored immediately if we detect any image color space
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Check for color-setting commands in the content stream (text color and graphics)
                        var contentBytes = reader.GetPageContent(i);
                        string contentString = Encoding.UTF8.GetString(contentBytes);

                        // Look for color-related operations in the content stream
                        if (contentString.Contains("/DeviceRGB") || contentString.Contains("/DeviceCMYK"))
                        {
                            return "Colored"; // Return Colored immediately if any color space is detected
                        }

                        // Check for color-setting commands like setrgbcolor or setcmykcolor in the page's content
                        if (contentString.Contains("setrgbcolor") || contentString.Contains("setcmykcolor"))
                        {
                            return "Colored"; // Return Colored if any color-setting operations are found
                        }

                        // Additional check for cases where color might be applied but not immediately obvious
                        if (contentString.Contains("rg") || contentString.Contains("k"))
                        {
                            return "Colored"; // Return Colored if there are any color commands (even partial ones)
                        }
                    }

                    // If no color found after checking all pages, return Greyscale
                    return "Greyscale";
                }
                catch
                {
                    return "Unknown"; // If we can't determine, return Unknown
            }
        }





        private void pdfFileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pdfFileListView.SelectedItem is FileItem selectedFile)
            {
                // Ensure correct parameters are passed to proceedPrinting
                var proceedPrintingWindow = new proceedPrinting(
                    selectedFile.FilePath,
                    selectedFile.FileName,
                    selectedFile.PaperSize,  // Pass paper size here
                    selectedFile.PageCount,  // Pass page count here
                    selectedFile.ColorStatus)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                bool? result = proceedPrintingWindow.ShowDialog();

                if (result == true)
                {
                    MessageBox.Show("User confirmed printing.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                   
                }

                pdfFileListView.SelectedItem = null;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            // Replace `HomeUserControl` with the appropriate UserControl for your home page
            mainWindow.MainContent.Content = new HomeUserControl();
        }



        // Helper class to hold file information
        public class FileItem
            {
                public string FileName { get; set; }
                public string FilePath { get; set; }
                public string PaperSize { get; set; }
                public string ColorStatus { get; set; }
                public int PageCount { get; set; }
            }
        }
    }
       
