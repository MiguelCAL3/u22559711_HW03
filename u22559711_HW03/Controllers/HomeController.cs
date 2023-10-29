using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using u22559711_HW03.Models;
using PagedList;
using PagedList.Mvc;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Rotativa;
using System.Diagnostics;
using iTextSharp.text;
using iTextSharp.text.pdf;
using GemBox.Document;

namespace u22559711_HW03.Controllers
{
    public class HomeController : Controller
    {
        private LibraryEntities1 db = new LibraryEntities1();
        public async Task<ActionResult> Index(int? studentPage, int? bookPage)
        {
            var students = db.students.ToList(); // Retrieve students from the database.
            var books = db.books.Include(p => p.borrows).ToList(); // Retrieve books from the database.


            int studentPageNumber = studentPage ?? 1;
            int bookPageNumber = bookPage ?? 1;
            int pageSize = 5;

            var studentViewModel = new CombinedViewModelSB
            {
                Students = students.ToPagedList(studentPageNumber, pageSize),
                Books = books.ToPagedList(bookPageNumber, pageSize)
            };

            return View(studentViewModel);
        }

        public async Task<ActionResult> Maintain(int? authorPage, int? borrowPage, int? typesPage)
        {
            var types = db.types.ToList();
            var borrows = db.borrows.Include(s => s.students).Include(b => b.books).ToList();
            var authors = db.authors.ToList();

            int authorPageNumber = authorPage ?? 1;
            int borrowPageNumber = borrowPage ?? 1;
            int typesPageNumber = typesPage ?? 1;
            int pageSize = 5;

            var ViewModel = new CombinedViewModelTAP
            {
                Author = authors.ToPagedList(authorPageNumber, pageSize),
                Borrow = borrows.ToPagedList(borrowPageNumber, pageSize),
                Type = types.ToPagedList(typesPageNumber, pageSize)
            };

            return View(ViewModel);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private List<ChartDataPoint> GetTop10BorrowedBooks()
        {
            var data = db.MostBorrowedBooks()
                .OrderByDescending(item => item.borrow_count)
                .Take(10)
                .ToList();

            var top10Books = data.Select(item => new ChartDataPoint
            {
                Value = (int)item.borrow_count,
                Label = item.book_name
            }).ToList();

            return top10Books;


        }

        private List<ChartDataPoint> GetTop10LeastBorrowedBooks()
        {
            var data = db.MostBorrowedBooks()
                .OrderBy(item => item.borrow_count) // Sort in ascending order
                .Take(10)
                .ToList();

            var top10Books = data.Select(item => new ChartDataPoint
            {
                Value = (int)item.borrow_count,
                Label = item.book_name
            }).ToList();

            return top10Books;
        }



        private List<ChartDataPoint> GetDataFromMostBorrowedBooks()
        {
            // Call your stored procedure or method to get the data
            var data = db.MostBorrowedBooks().ToList();

            // Transform the retrieved data into ChartDataPoint objects
            var chartData = data.Select(item => new ChartDataPoint
            {
                Label = item.book_name,
                Value = (int)item.borrow_count
            }).ToList();

            return chartData;
        }

        

        //USE THIS
        public ActionResult ReportSection()
        {
            var data = GetDataFromMostBorrowedBooks();
            var top10Books = GetTop10BorrowedBooks();
            var top10leastBooks = GetTop10LeastBorrowedBooks();
            ViewBag.Top10leastBooks = JsonConvert.SerializeObject(top10leastBooks);
            ViewBag.Top10Books = JsonConvert.SerializeObject(top10Books);
            ViewBag.ChartData = JsonConvert.SerializeObject(data);

            // Create an instance of CombinedReportViewModel
            var viewModel = new CombinedReportViewModel();

            // Populate the FileList property of the view model with files
            viewModel.FileList = new List<FileModel>();
            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Documents"));

            

            foreach (string filePath in filePaths)
            {

                viewModel.FileList.Add(new FileModel { FileName = Path.GetFileName(filePath)});
               
            }

            return View("ReportSection", viewModel);
        }





        [HttpPost]
        public FileResult SAVE(CombinedReportViewModel model)
        {
            // If using Professional version, put your serial key below.
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            var templateFile = Server.MapPath("~/App_Data/DocumentTemplate.docx");

            // Load template document.
            var document = DocumentModel.Load(templateFile);
 
            // Insert content from HTML editor.
            var bookmark = document.Bookmarks["HtmlBookmark"];
            bookmark.GetContent(true).LoadText(model.SingleFile.Content, LoadOptions.HtmlDefault);

            // Save document to stream in specified format.
            var saveOptions = GetSaveOptions(model.SingleFile.Extension);
            var stream = new MemoryStream();
            document.Save(stream, saveOptions);

            // Download document.
            var downloadDirectory = Server.MapPath("~/Documents/");
            var downloadFile = $"{model.SingleFile.FileName}{model.SingleFile.Extension}";
            

            // Check if the directory exists; if not, create it.
            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }


            var fullPath = Path.Combine(downloadDirectory, downloadFile);
            System.IO.File.WriteAllBytes(fullPath, stream.ToArray());

            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Documents/"));
            List<FileModel> files = new List<FileModel>();
            foreach (string filePath in filePaths)
            {

                    files.Add(new FileModel { FileName = Path.GetFileName(filePath) });

                
            }
            
            
            return File(fullPath, saveOptions.ContentType, downloadFile);
            
        }


        private static SaveOptions GetSaveOptions(string extension)
        {
            switch (extension)
            {
                case ".docx": return SaveOptions.DocxDefault;
                case ".pdf": return SaveOptions.PdfDefault;
                case ".xps": return SaveOptions.XpsDefault;
                case ".html": return SaveOptions.HtmlDefault;
                case ".mhtml": return new HtmlSaveOptions() { HtmlType = HtmlType.Mhtml };
                case ".rtf": return SaveOptions.RtfDefault;
                case ".xml": return SaveOptions.XmlDefault;
                case ".png": return SaveOptions.ImageDefault;
                case ".jpeg": return new ImageSaveOptions(ImageSaveFormat.Jpeg);
                case ".gif": return new ImageSaveOptions(ImageSaveFormat.Gif);
                case ".bmp": return new ImageSaveOptions(ImageSaveFormat.Bmp);
                case ".tiff": return new ImageSaveOptions(ImageSaveFormat.Tiff);
                case ".wmp": return new ImageSaveOptions(ImageSaveFormat.Wmp);
                default: return SaveOptions.TxtDefault;
            }
        }

        public FileResult DownloadFile(string fileName)
        {
            string path = Server.MapPath("~/Documents/") + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", fileName);
        }

        public ActionResult DeleteFile(string fileName)
        {
            string path = Server.MapPath("~/Documents/") + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);


            System.IO.File.Delete(path);

            return RedirectToAction("ReportSection");
        }



        //DONT USE THIS, THIS WAS TEST, GO TO REPORT SECTION, IGNORE BELOW
        public ActionResult InteractiveChart()
        {
            var data = GetDataFromMostBorrowedBooks();
            var top10Books = GetTop10BorrowedBooks();
            var top10leastBooks = GetTop10LeastBorrowedBooks();
            ViewBag.Top10leastBooks = JsonConvert.SerializeObject(top10leastBooks);
            ViewBag.Top10Books = JsonConvert.SerializeObject(top10Books);
            ViewBag.ChartData = JsonConvert.SerializeObject(data);
            return View();
        }

    }
}
    