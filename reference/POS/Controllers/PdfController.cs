using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POS.Controllers
{
    public class PdfController : BaseController
    {
        [HttpPost]
        public ActionResult Download()
        {
            string html = Request.Unvalidated["DocumentName"];
            string fileName = Request.Unvalidated["FileName"];
            IronPdf.HtmlToPdf Renderer = new IronPdf.HtmlToPdf();
            Renderer.PrintOptions.MarginLeft = 10;
            Renderer.PrintOptions.MarginRight = 10;
            Renderer.PrintOptions.FitToPaperWidth = true;
            Renderer.PrintOptions.FirstPageNumber = 1;
            var PDF = Renderer.RenderHtmlAsPdf(html);
            return File(PDF.BinaryData, "application/pdf", fileName + ".pdf");
        }
    }
}