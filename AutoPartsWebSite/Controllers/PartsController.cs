using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoPartsWebSite.Models;
using System.IO;
using Microsoft.AspNet.Identity;
using IdentityAutoPart.Models;
using Microsoft.AspNet.Identity.Owin;

namespace AutoPartsWebSite.Controllers
{
    public class PartsController : Controller
    {
        private PartModel db = new PartModel();

        // GET: Parts
        public ActionResult Index()
        {
            return View(db.Parts.Take(100).ToList());
        }

        // GET: Parts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Part part = db.Parts.Find(id);
            if (part == null)
            {
                return HttpNotFound();
            }
            return View(part);
        }

        // GET: Parts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Parts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ImportId,Brand,Number,Name,Details,Size,Weight,Quantity,Price,Supplier,DeliveryTime")] Part part)
        {
            if (ModelState.IsValid)
            {
                db.Parts.Add(part);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(part);
        }

        // GET: Parts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Part part = db.Parts.Find(id);
            if (part == null)
            {
                return HttpNotFound();
            }
            return View(part);
        }

        // POST: Parts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ImportId,Brand,Number,Name,Details,Size,Weight,Quantity,Price,Supplier,DeliveryTime")] Part part)
        {
            if (ModelState.IsValid)
            {
                db.Entry(part).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(part);
        }

        // GET: Parts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Part part = db.Parts.Find(id);
            if (part == null)
            {
                return HttpNotFound();
            }
            return View(part);
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Part part = db.Parts.Find(id);
            db.Parts.Remove(part);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        

        ////[Authorize(Roles = "RegistredUser")]
        //public ActionResult SearchParts(string autopartNumbers) //, int? maxItemCount)
        //{
        //    string[] autopartNumbersList = new string[] { };
        //    int maxItemCount = GetSearchLimit(); // get info from db about curren user search limit
        //    ViewBag.SearchLimit = maxItemCount;
        //    var autoparts = (from s in db.Parts
        //                     select s).Take(0);

        //    if (!String.IsNullOrEmpty(autopartNumbers))
        //    {
        //        // string txt = TextBox1.Text;
        //        string[] delimiters = { Environment.NewLine, ".", ",", ";", " " }; //new Char[] { '\n', '\r' }
        //        autopartNumbersList = autopartNumbers.Split(delimiters, StringSplitOptions.RemoveEmptyEntries); // StringSplitOptions.None

        //        var numbersList = new List<string>(autopartNumbersList);

        //        // two digits for search item message
        //        ViewBag.SearchLimit = maxItemCount;
        //        ViewBag.ItemsToSearch = numbersList.Count;

        //        if (maxItemCount < numbersList.Count)
        //        {
        //            numbersList.RemoveRange((int)maxItemCount, numbersList.Count() - (int)maxItemCount);
        //        }
        //        autopartNumbersList = numbersList.ToArray();

        //        //foreach (string autopartNumber in autopartNumbersList)
        //        //{
        //        //    if (!String.IsNullOrEmpty(autopartNumber))                 
        //        //    {
        //        //        autoparts = autoparts.Where(c => c.Number.Contains(autopartNumbersList));                       
        //        //    }
        //        //}


        //        //assemble an array of ID values
        //        //int[] customerIds = new int[] { 1, 2, 3 };
        //        //string autopartNumbersString = autopartNumbersList.ToString(", ");

        //        autoparts = (from s in db.Parts
        //                     where autopartNumbersList.Contains(s.Number)
        //                     select s).Take(1000);

        //        //autoparts = autoparts.Where(c => c.Number.Contains(autopartNumbersList));

        //    }
        //    foreach (Part part in autoparts)
        //    {
        //        part.Price = CalcUserPrice(part.Id);
        //        part.Quantity = CalcUserQuantity(part.Id);
        //    }
        //    //Session["AutopartsSearchResult"] = autoparts; //.ToList();
        //    Session["AutopartNumbersList"] = autopartNumbersList;
        //    return View(autoparts);
        //}

        //[Authorize(Roles = "RegistredUser")]
        public ActionResult Search(string autopartNumbers) 
        {
            string[] autopartNumbersList = new string[] { };
            int maxItemCount = GetSearchLimit(); // get info from db about curren user search limit
            ViewBag.SearchLimit = maxItemCount;

            if (String.IsNullOrEmpty(autopartNumbers))
            {
                TempData["shortMessage"] = "Данных не обнаружено, уточните запрос."; //"Тут рыбы нет !";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string[] delimiters = { Environment.NewLine, ".", ",", ";", " " }; //new Char[] { '\n', '\r' }
                autopartNumbersList = autopartNumbers.Split(delimiters, StringSplitOptions.RemoveEmptyEntries); // StringSplitOptions.None

                var numbersList = new List<string>(autopartNumbersList);

                // two digits for search item message
                ViewBag.SearchLimit = maxItemCount;
                ViewBag.ItemsToSearch = numbersList.Count;

                if (maxItemCount < numbersList.Count)
                {
                    numbersList.RemoveRange((int)maxItemCount, numbersList.Count() - (int)maxItemCount);
                }
                // find and add replacement to numbers list
                var autopartsReplacement = (from s in db.PartReplacement
                                            where numbersList.Contains(s.Number)
                                            select s.Replacement).ToList();
                numbersList.AddRange(autopartsReplacement);
                // create array from list
                autopartNumbersList = numbersList.ToArray();

                //get standard (not hidden) suppliers
                var stdSuppliers = (from ss in db.Suppliers
                                   where ss.TypeId == 1 // standard
                                   select ss.Id).ToList();

                // read about ".Select(x => new Part"  by link  
                // http://stackoverflow.com/questions/5325797/the-entity-cannot-be-constructed-in-a-linq-to-entities-query
                var autoparts = (from p in db.Parts
                                 join a in db.PartAliases on p.Number equals a.Number into ps
                                 from a in ps.DefaultIfEmpty()
                                 where autopartNumbersList.Contains(p.Number) 
                                    && stdSuppliers.Contains(p.SupplierId)
                                    
                                 //select p
                                 select new 
                                 {
                                     Id = p.Id,
                                     ImportId = p.ImportId,
                                     Brand = p.Brand,
                                     Number = p.Number,
                                     Name = !string.IsNullOrEmpty(a.Name) ? a.Name : p.Name,
                                     Details = !string.IsNullOrEmpty(a.Details) ? a.Details : p.Details,
                                     Size = !string.IsNullOrEmpty(a.Size) ? a.Size : p.Size,
                                     Weight = !string.IsNullOrEmpty(a.Weight) ? a.Weight : p.Weight,
                                     Quantity = p.Quantity,
                                     Price = p.Price,
                                     //Supplier = p.Supplier,
                                     //DeliveryTime = p.DeliveryTime,
                                     SupplierId = p.SupplierId
                                 }
                                 ).ToList()
                                 .Select(x => new Part
                                 {
                                     Id = x.Id,
                                     ImportId = x.ImportId,
                                     Brand = x.Brand,
                                     Number = x.Number,
                                     Name = x.Name,
                                     Details = x.Details,
                                     Size = x.Size,
                                     Weight = x.Weight,
                                     Quantity = x.Quantity,
                                     Price = x.Price,
                                     //Supplier = x.Supplier,
                                     //DeliveryTime = x.DeliveryTime,
                                     SupplierId = x.SupplierId
                                 }).Take(1000); 
                if (autoparts.Count() <= 0) // not found any numbers
                {
                    TempData["shortMessage"] = "Данных не обнаружено, уточните запрос."; //"Тут рыбы нет !";
                    return RedirectToAction("Index", "Home");
                }

                List<Part> aparts = new List<Part> { };
                foreach (Part part in autoparts)
                {
                    part.Price = CalcUserPrice(part.Id);
                    part.Quantity = CalcUserQuantity(part.Id);
                    aparts.Add(part);
                }
                autoparts = aparts;

                Session["AutopartNumbersList"] = autopartNumbersList;
                Session["AutopartSearchResult"] = autoparts;
                return View((IEnumerable<Part>)autoparts);
            }
        }

        class SearchFileCriteria
        {
            // Auto-implemented properties.
            public string Number { get; set; }
            public string Brand { get; set; }
            public string Amount { get; set; }
            public string Reference1 { get; set; }
            public string Reference2 { get; set; }
        }
                
        [Authorize(Roles = "RegistredUser")]
        public ActionResult SearchFromFile(int? DeliveryTime, bool? UseReplacement, bool? UseAmount, HttpPostedFileBase upload)
        {
            // -------------------------- begin find search criteria   ----------------------------------------
            // list for search creterias
            List<SearchFileCriteria> SearchFileCriterias = new List<SearchFileCriteria> { };

            string uploadFileName = "";
            // take search creteria from session
            if (upload == null && Session["SearchFileCriterias"] != null)
            {
                
                List<SearchFileCriteria> sessionSearch = (List<SearchFileCriteria>)Session["SearchFileCriterias"];
                if (sessionSearch.Count() > 0)
                {
                    SearchFileCriterias = sessionSearch;
                    uploadFileName = (string)Session["uploadFileName"];
                }
            }

            //import files into search creterias list
            int firstDataRow = 2;
            try
            {
                if (upload != null && upload.ContentLength > 0)
                {                    
                    // load from stream
                    using (ExcelPackage package = new ExcelPackage(upload.InputStream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        for (int i = firstDataRow; i <= worksheet.Dimension.End.Row; i++)
                        {
                            SearchFileCriteria SearchFileCriteriaItem = new SearchFileCriteria
                            {
                                Number = (worksheet.Cells["A" + i.ToString()].Value != null) ? worksheet.Cells["A" + i.ToString()].Value.ToString() : "",
                                Brand = (worksheet.Cells["B" + i.ToString()].Value != null) ? worksheet.Cells["B" + i.ToString()].Value.ToString() : "",
                                Amount = (worksheet.Cells["C" + i.ToString()].Value != null) ? worksheet.Cells["C" + i.ToString()].Value.ToString() : "",
                                ////!string.IsNullOrEmpty(worksheet.Cells["C" + i.ToString()].Value.ToString()) ? Convert.ToInt32(worksheet.Cells["C" + i.ToString()].Value) : 0,
                                Reference1 = (worksheet.Cells["D" + i.ToString()].Value != null) ? worksheet.Cells["D" + i.ToString()].Value.ToString() : "",
                                Reference2 = (worksheet.Cells["E" + i.ToString()].Value != null) ? worksheet.Cells["E" + i.ToString()].Value.ToString() : ""
                            };                               
                            SearchFileCriterias.Add(SearchFileCriteriaItem);
                        }
                    }
                    // finish    
                    uploadFileName = upload.FileName;
                    Session["SearchFileCriterias"] = SearchFileCriterias;
                    Session["uploadFileName"] = uploadFileName;
                }                
            }
            catch (Exception ex)
            {
                TempData["shortMessage"] = "Ошибка в файле поиска:" + ex.Message.ToString();
                return RedirectToAction("IndexFile", "Home");
            }
            // --------------------------  end find search criteria   ----------------------------------------

            string autopartNumbers = "";
            //copy part numbers to array
            foreach (var searchCriteria in SearchFileCriterias)
            {
                autopartNumbers += searchCriteria.Number + ", ";
            }

            string[] autopartNumbersList = new string[] { };
            int maxItemCount = GetSearchLimit(); // get info from db about curren user search limit
            ViewBag.SearchLimit = maxItemCount;

            if (String.IsNullOrEmpty(autopartNumbers))
            {
                TempData["shortMessage"] = "Данных не обнаружено, уточните запрос."; //"Тут рыбы нет !";
                return RedirectToAction("IndexFile", "Home");
            }
            else
            {
                string[] delimiters = { Environment.NewLine, ".", ",", ";", " " }; //new Char[] { '\n', '\r' }
                autopartNumbersList = autopartNumbers.Split(delimiters, StringSplitOptions.RemoveEmptyEntries); // StringSplitOptions.None

                var numbersList = new List<string>(autopartNumbersList);

                // two digits for search item message
                ViewBag.SearchLimit = maxItemCount;
                ViewBag.ItemsToSearch = numbersList.Count;

                if (maxItemCount < numbersList.Count)
                {
                    numbersList.RemoveRange((int)maxItemCount, numbersList.Count() - (int)maxItemCount);
                }
                // find and add replacement to numbers list
                if ((bool)UseReplacement)
                {
                    var autopartsReplacement = (from s in db.PartReplacement
                                            where numbersList.Contains(s.Number)
                                            select s.Replacement).ToList();
                    numbersList.AddRange(autopartsReplacement);
                }
                // create array from list
                autopartNumbersList = numbersList.ToArray();

                //get suppliers for selection
                var selSuppliers = (from ss in db.Suppliers
                                    where ss.TypeId == 1 // standard
                                    select ss.Id).ToList();

                // read about ".Select(x => new Part"  by link  
                // http://stackoverflow.com/questions/5325797/the-entity-cannot-be-constructed-in-a-linq-to-entities-query
                var autoparts = (from p in db.Parts
                                 join a in db.PartAliases on p.Number equals a.Number into ps
                                 from a in ps.DefaultIfEmpty()
                                 where autopartNumbersList.Contains(p.Number)
                                    && selSuppliers.Contains(p.SupplierId)

                                 select new
                                 {
                                     Id = p.Id,
                                     ImportId = p.ImportId,
                                     Brand = p.Brand,
                                     Number = p.Number,
                                     Name = !string.IsNullOrEmpty(a.Name) ? a.Name : p.Name,
                                     Details = !string.IsNullOrEmpty(a.Details) ? a.Details : p.Details,
                                     Size = !string.IsNullOrEmpty(a.Size) ? a.Size : p.Size,
                                     Weight = !string.IsNullOrEmpty(a.Weight) ? a.Weight : p.Weight,
                                     Quantity = p.Quantity,
                                     Price = p.Price,
                                     //Supplier = p.Supplier,
                                     //DeliveryTime = p.DeliveryTime,
                                     SupplierId = p.SupplierId
                                 }
                                 ).ToList()
                                 .Select(x => new Part
                                 {
                                     Id = x.Id,
                                     ImportId = x.ImportId,
                                     Brand = x.Brand,
                                     Number = x.Number,
                                     Name = x.Name,
                                     Details = x.Details,
                                     Size = x.Size,
                                     Weight = x.Weight,
                                     Quantity = x.Quantity,
                                     Price = x.Price,
                                     //Supplier = x.Supplier,
                                     //DeliveryTime = x.DeliveryTime,
                                     SupplierId = x.SupplierId
                                 }).Take(1000);
                if (autoparts.Count() <= 0) // not found any numbers
                {
                    TempData["shortMessage"] = "Данных не обнаружено, уточните запрос."; //"Тут рыбы нет !";
                    return RedirectToAction("IndexFile", "Home");
                }

                foreach (Part part in autoparts)
                {
                    part.Price = CalcUserPrice(part.Id);
                    part.Quantity = CalcUserQuantity(part.Id);
                }

                // --------------------------  begin add to Cart   ----------------------------------------
                // create new preCart for display items before add them to card
                List<Cart> preCart = new List<Cart> { };

                foreach (SearchFileCriteria sc in SearchFileCriterias)
                {
                    List<string> searchNumbers = new List<string> { sc.Number};

                    // find and add replacement to numbers list
                    if ((bool)UseReplacement)
                    {
                        var apr = (from r in db.PartReplacement
                                   where r.Number.Contains(sc.Number)
                                   select r.Replacement).ToList();
                        searchNumbers.AddRange(apr);
                    }                    

                    var aps = from p in autoparts
                              where searchNumbers.Contains(p.Number)
                              select p;
                    aps = aps.Where(p => p.Brand.Contains(sc.Brand));                 
                    aps = aps.OrderByDescending(p => p.Price); // sort by price
                    if (aps != null)
                    {
                        foreach (Part ap in aps)
                        {
                            if (!string.IsNullOrEmpty(ap.DeliveryTime)
                                && !string.IsNullOrWhiteSpace(ap.DeliveryTime)
                                && Convert.ToInt32(ap.DeliveryTime) <= (int)DeliveryTime)
                            {
                                if (!(bool)UseAmount)
                                {
                                    // add to cart and exit                          
                                    //AddCartItem(ap.Id, Convert.ToInt32(sc.Amount), sc.Reference1, sc.Reference2);

                                    // add to precart and exit
                                    preCart.Add(NewPreCartItem(ap.Id, Convert.ToInt32(sc.Amount), sc.Reference1, sc.Reference2));
                                    break;
                                }
                                if (Convert.ToInt32(ap.Quantity) >= Convert.ToInt32(sc.Amount))
                                {
                                    // add to cart and exit                          
                                    //AddCartItem(ap.Id, Convert.ToInt32(sc.Amount), sc.Reference1, sc.Reference2);

                                    // add to precart and exit
                                    preCart.Add(NewPreCartItem(ap.Id, Convert.ToInt32(sc.Amount), sc.Reference1, sc.Reference2));
                                    break;
                                }
                                else
                                {
                                    // add to cart and continue loop
                                    //AddCartItem(ap.Id, Convert.ToInt32(ap.Quantity), sc.Reference1, sc.Reference2);

                                    // add to precart and continue loop
                                    preCart.Add(NewPreCartItem(ap.Id, Convert.ToInt32(ap.Quantity), sc.Reference1, sc.Reference2));
                                    sc.Amount = (Convert.ToInt32(sc.Amount) - Convert.ToInt32(ap.Quantity)).ToString();
                                }
                            }
                        }
                    }
                }
                // --------------------------  end add to Cart   ----------------------------------------

                ViewBag.FileName = uploadFileName;
                ViewBag.DeliveryTime = DeliveryTime;
                ViewBag.UseReplacement = UseReplacement;
                ViewBag.UseAmount = UseAmount;

                Session["AutopartNumbersList"] = autopartNumbersList;
                Session["AutopartSearchResult"] = preCart;

                // return RedirectToAction("Index","Carts");
                return View((IEnumerable<Cart>)preCart.OrderBy(x=>x.Price));
            }
        }

        private Cart NewPreCartItem(int PartId, int Amount, string Reference1, string Reference2)
        {
            string currentUserId = User.Identity.GetUserId();
            var userCart = (from s in db.Carts
                            select s).Take(1000);
            userCart = userCart.Where(s => s.UserId.Equals(currentUserId));

            Part autopart = db.Parts.Find(PartId);
            Cart cart = new Cart();
            if ((autopart != null) && (Amount != 0))
            {
                cart.PartId = autopart.Id;
                cart.UserId = User.Identity.GetUserId();
                cart.Brand = autopart.Brand;
                cart.Number = autopart.Number;
                cart.Name = autopart.Name;
                cart.Details = autopart.Details;
                cart.Size = autopart.Size;
                cart.Weight = autopart.Weight;
                cart.Quantity = autopart.Quantity;
                cart.Supplier = autopart.Supplier;
                cart.Price = CalcUserPrice(autopart.Id); // autopart.Price;
                cart.DeliveryTime = autopart.DeliveryTime;
                cart.Amount = Amount;
                cart.Data = DateTime.Now;
                cart.BasePrice = autopart.Price;
                cart.Reference1 = Reference1;
                cart.Reference2 = Reference2;
            }
            return cart;
        }

        private void AddCartItem(int PartId, int Amount, string Reference1, string Reference2)
        {
            string currentUserId = User.Identity.GetUserId();
            var userCart = (from s in db.Carts
                            select s).Take(1000);
            userCart = userCart.Where(s => s.UserId.Equals(currentUserId));

            Part autopart = db.Parts.Find(PartId);
            Cart cartpart = userCart.Where(s => s.PartId == PartId).FirstOrDefault();
            if ((cartpart != null) && (autopart != null) && (Amount != 0))
            {
                if (Convert.ToInt32(cartpart.Quantity) >= (cartpart.Amount + Amount))
                {
                    cartpart.Amount = cartpart.Amount + Amount;
                    cartpart.Reference1 = Reference1;
                    cartpart.Reference2 = Reference2;
                    if (ModelState.IsValid)
                    {
                        cartpart.Data = DateTime.Now;
                        db.Entry(cartpart).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                if ((autopart != null) && (Amount != 0))
                {
                    Cart cart = new Cart();
                    cart.PartId = autopart.Id;
                    cart.UserId = User.Identity.GetUserId();

                    cart.Brand = autopart.Brand;
                    cart.Number = autopart.Number;
                    cart.Name = autopart.Name;
                    cart.Details = autopart.Details;
                    cart.Size = autopart.Size;
                    cart.Weight = autopart.Weight;
                    cart.Quantity = autopart.Quantity;
                    cart.Supplier = autopart.Supplier;
                    cart.Price = CalcUserPrice(autopart.Id); // autopart.Price;
                    cart.DeliveryTime = autopart.DeliveryTime;
                    cart.Amount = Amount;
                    cart.Data = DateTime.Now;
                    cart.BasePrice = autopart.Price;
                    cart.Reference1 = Reference1;
                    cart.Reference2 = Reference2;

                    if (ModelState.IsValid)
                    {
                        db.Carts.Add(cart);
                        db.SaveChanges();
                    }
                }
            }

        }

       

        class SearchFileCriteriaAdmin
        {
            // Auto-implemented properties.
            public string Number { get; set; }
            public string Brand { get; set; }
            public string Amount { get; set; }            
            public string Reference1 { get; set; }
            public string Reference2 { get; set; }
            public string Price { get; set; }
            public string Details { get; set; }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SearchFromFileAdmin(int? DeliveryTime, bool? UseReplacement, bool? UseAmount, bool? UseHidden, HttpPostedFileBase upload)
        {
            // -------------------------- begin find search criteria   ----------------------------------------
            // list for search creterias
            List<SearchFileCriteriaAdmin> SearchFileCriterias = new List<SearchFileCriteriaAdmin> { };

            //import files into search creterias list
            int firstDataRow = 2;
            try
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    // load from stream
                    using (ExcelPackage package = new ExcelPackage(upload.InputStream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        for (int i = firstDataRow; i <= worksheet.Dimension.End.Row; i++)
                        {
                            SearchFileCriteriaAdmin SearchFileCriteriaItem = new SearchFileCriteriaAdmin
                            {
                                Number = (worksheet.Cells["A" + i.ToString()].Value != null) ? worksheet.Cells["A" + i.ToString()].Value.ToString() : "",
                                Brand = (worksheet.Cells["B" + i.ToString()].Value != null) ? worksheet.Cells["B" + i.ToString()].Value.ToString() : "",
                                Amount = (worksheet.Cells["C" + i.ToString()].Value != null) ? worksheet.Cells["C" + i.ToString()].Value.ToString() : "",
                                Reference1 = (worksheet.Cells["D" + i.ToString()].Value != null) ? worksheet.Cells["D" + i.ToString()].Value.ToString() : "",
                                Reference2 = (worksheet.Cells["E" + i.ToString()].Value != null) ? worksheet.Cells["E" + i.ToString()].Value.ToString() : "",
                                Price = (worksheet.Cells["F" + i.ToString()].Value != null) ? worksheet.Cells["F" + i.ToString()].Value.ToString() : "",
                                Details = (worksheet.Cells["G" + i.ToString()].Value != null) ? worksheet.Cells["G" + i.ToString()].Value.ToString() : ""
                            };
                            SearchFileCriterias.Add(SearchFileCriteriaItem);
                        }
                    }
                    // finish                    
                }
            }
            catch (Exception ex)
            {
                TempData["shortMessage"] = "Ошибка в файле поиска:" + ex.Message.ToString();
                return RedirectToAction("IndexFileAdmin", "Home");
            }
            // --------------------------  end find search criteria   ----------------------------------------

            string autopartNumbers = "";
            //copy part numbers to array
            foreach (var searchCriteria in SearchFileCriterias)
            {
                autopartNumbers += searchCriteria.Number + ", ";
            }

            string[] autopartNumbersList = new string[] { };
            int maxItemCount = GetSearchLimit(); // get info from db about curren user search limit
            ViewBag.SearchLimit = maxItemCount;

            if (String.IsNullOrEmpty(autopartNumbers))
            {
                TempData["shortMessage"] = "Данных не обнаружено, уточните запрос."; //"Тут рыбы нет !";
                return RedirectToAction("IndexFileAdmin", "Home");
            }
            else
            {
                string[] delimiters = { Environment.NewLine, ".", ",", ";", " " }; //new Char[] { '\n', '\r' }
                autopartNumbersList = autopartNumbers.Split(delimiters, StringSplitOptions.RemoveEmptyEntries); // StringSplitOptions.None

                var numbersList = new List<string>(autopartNumbersList);

                // two digits for search item message
                ViewBag.SearchLimit = maxItemCount;
                ViewBag.ItemsToSearch = numbersList.Count;

                if (maxItemCount < numbersList.Count)
                {
                    numbersList.RemoveRange((int)maxItemCount, numbersList.Count() - (int)maxItemCount);
                }
                // find and add replacement to numbers list
                if ((bool)UseReplacement)
                {
                    var autopartsReplacement = (from s in db.PartReplacement
                                                where numbersList.Contains(s.Number)
                                                select s.Replacement).ToList();
                    numbersList.AddRange(autopartsReplacement);
                }
                // create array from list
                autopartNumbersList = numbersList.ToArray();

                //get suppliers for selection
                var selSuppliers = (from ss in db.Suppliers
                                        where ss.TypeId == 1 // standard
                                        select ss.Id).ToList();
                if ((bool)UseHidden)
                {
                    selSuppliers = (from ss in db.Suppliers
                                    where ss.TypeId == 1 // standard
                                        || ss.TypeId == 2 // Hidden
                                    select ss.Id).ToList();
                }

                // read about ".Select(x => new Part"  by link  
                // http://stackoverflow.com/questions/5325797/the-entity-cannot-be-constructed-in-a-linq-to-entities-query
                var autoparts = (from p in db.Parts
                                 join a in db.PartAliases on p.Number equals a.Number into ps
                                 from a in ps.DefaultIfEmpty()
                                 where autopartNumbersList.Contains(p.Number)
                                    && selSuppliers.Contains(p.SupplierId)

                                 select new
                                 {
                                     Id = p.Id,
                                     ImportId = p.ImportId,
                                     Brand = p.Brand,
                                     Number = p.Number,
                                     Name = !string.IsNullOrEmpty(a.Name) ? a.Name : p.Name,
                                     Details = !string.IsNullOrEmpty(a.Details) ? a.Details : p.Details,
                                     Size = !string.IsNullOrEmpty(a.Size) ? a.Size : p.Size,
                                     Weight = !string.IsNullOrEmpty(a.Weight) ? a.Weight : p.Weight,
                                     Quantity = p.Quantity,
                                     Price = p.Price,
                                     //Supplier = p.Supplier,
                                     //DeliveryTime = p.DeliveryTime,
                                     SupplierId = p.SupplierId
                                 }
                                 ).ToList()
                                 .Select(x => new Part
                                 {
                                     Id = x.Id,
                                     ImportId = x.ImportId,
                                     Brand = x.Brand,
                                     Number = x.Number,
                                     Name = x.Name,
                                     Details = x.Details,
                                     Size = x.Size,
                                     Weight = x.Weight,
                                     Quantity = x.Quantity,
                                     Price = x.Price,
                                     //Supplier = x.Supplier,
                                     //DeliveryTime = x.DeliveryTime,
                                     SupplierId = x.SupplierId
                                 }).Take(1000);
                if (autoparts.Count() <= 0) // not found any numbers
                {
                    TempData["shortMessage"] = "Данных не обнаружено, уточните запрос."; //"Тут рыбы нет !";
                    return RedirectToAction("IndexFileAdmin", "Home");
                }

                foreach (Part part in autoparts)
                {
                    part.Price = CalcUserPrice(part.Id);
                    part.Quantity = CalcUserQuantity(part.Id);
                }

                // --------------------------  begin add to Cart   ----------------------------------------

                foreach (SearchFileCriteriaAdmin sc in SearchFileCriterias)
                {
                    List<string> searchNumbers = new List<string> { sc.Number };

                    // find and add replacement to numbers list
                    if ((bool)UseReplacement)
                    {
                        var apr = (from r in db.PartReplacement
                                   where r.Number.Contains(sc.Number)
                                   select r.Replacement).ToList();
                        searchNumbers.AddRange(apr);
                    }

                    var aps = from p in autoparts
                              where searchNumbers.Contains(p.Number)
                              select p;
                    aps = aps.Where(p => p.Brand.Contains(sc.Brand));
                    aps = aps.OrderBy(p => p.Price); // sort by price
                    if (aps != null)
                    {
                        foreach (Part ap in aps)
                        {
                            if (!string.IsNullOrEmpty(ap.DeliveryTime)
                                && !string.IsNullOrWhiteSpace(ap.DeliveryTime)
                                && Convert.ToInt32(ap.DeliveryTime) <= (int)DeliveryTime)
                            {
                                if (!(bool)UseAmount)
                                {
                                    // add to cart and exit                          
                                    AddCartItemAdmin(ap.Id, Convert.ToInt32(sc.Amount), sc.Reference1, sc.Reference2, sc.Price, sc.Details);
                                    break;
                                }
                                if (Convert.ToInt32(ap.Quantity) >= Convert.ToInt32(sc.Amount))
                                {
                                    // add to cart and exit                          
                                    AddCartItemAdmin(ap.Id, Convert.ToInt32(sc.Amount), sc.Reference1, sc.Reference2, sc.Price, sc.Details);
                                    break;
                                }
                                else
                                {
                                    // add to cart and continue loop
                                    AddCartItemAdmin(ap.Id, Convert.ToInt32(ap.Quantity), sc.Reference1, sc.Reference2, sc.Price, sc.Details);
                                    sc.Amount = (Convert.ToInt32(sc.Amount) - Convert.ToInt32(ap.Quantity)).ToString();
                                }
                            }
                        }
                    }
                }
                // --------------------------  end add to Cart   ----------------------------------------

                ViewBag.FileName = upload.FileName;
                ViewBag.DeliveryTime = DeliveryTime;
                ViewBag.UseReplacement = UseReplacement;
                ViewBag.UseAmount = UseAmount;


                Session["AutopartNumbersList"] = autopartNumbersList;
                Session["AutopartSearchResult"] = autoparts;


                return RedirectToAction("IndexAdmin", "Carts");
            }
        }

        private void AddCartItemAdmin(int PartId, int Amount, string Reference1, string Reference2, string Price, string Details)
        {
            string currentUserId = User.Identity.GetUserId();
            var userCart = (from s in db.Carts
                            select s).Take(1000);
            userCart = userCart.Where(s => s.UserId.Equals(currentUserId));

            Part autopart = db.Parts.Find(PartId);
            Cart cartpart = userCart.Where(s => s.PartId == PartId).FirstOrDefault();
            if ((cartpart != null) && (autopart != null) && (Amount != 0))
            {
                if (Convert.ToInt32(cartpart.Quantity) >= (cartpart.Amount + Amount))
                {
                    cartpart.Amount = cartpart.Amount + Amount;
                    cartpart.Reference1 = Reference1;
                    cartpart.Reference2 = Reference2;
                    if (ModelState.IsValid)
                    {
                        cartpart.Data = DateTime.Now;
                        db.Entry(cartpart).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                if ((autopart != null) && (Amount != 0))
                {
                    Cart cart = new Cart();
                    cart.PartId = autopart.Id;
                    cart.UserId = User.Identity.GetUserId();

                    cart.Brand = autopart.Brand;
                    cart.Number = autopart.Number;
                    cart.Name = autopart.Name;
                    cart.Details = !string.IsNullOrEmpty(Details) ? Details : autopart.Details;
                    cart.Size = autopart.Size;
                    cart.Weight = autopart.Weight;
                    cart.Quantity = autopart.Quantity;
                    cart.Supplier = autopart.Supplier;
                    cart.Price = !string.IsNullOrEmpty(Price) ? Price: CalcUserPrice(autopart.Id); // autopart.Price;
                    cart.DeliveryTime = autopart.DeliveryTime;
                    cart.Amount = Amount;
                    cart.Data = DateTime.Now;
                    cart.BasePrice = autopart.Price;
                    cart.Reference1 = Reference1;
                    cart.Reference2 = Reference2;

                    if (ModelState.IsValid)
                    {
                        db.Carts.Add(cart);
                        db.SaveChanges();
                    }
                }
            }

        }

        private void FindPart(List<SearchFileCriteria> searchCriterias, List<Part> aParts)
        {
            CartsController cc = new CartsController { };
            foreach (SearchFileCriteria sc in searchCriterias)
            {               
                var aps = from p in aParts
                                where p.Number.Contains(sc.Number)
                                select p;
                aps = aps.OrderBy(p => p.Price); // sort by price
                if (aps != null )
                {
                    foreach(Part ap in aps)
                    {
                        if (Convert.ToInt32(ap.Quantity) >= Convert.ToInt32(sc.Amount))
                        {                            
                            // add to cart and exit                          
                            cc.AddToCartItem(ap.Id, Convert.ToInt32(sc.Amount), sc.Reference1, sc.Reference2);
                            break;
                        }
                        else
                        {
                            // add to cart and continue loop
                            cc.AddToCartItem(ap.Id, Convert.ToInt32(ap.Quantity), sc.Reference1, sc.Reference2);
                            sc.Amount = (Convert.ToInt32(sc.Amount) - Convert.ToInt32(ap.Quantity)).ToString();
                        }


                    }
                    
                }
            }

            return ;
        }

        //private ActionResult SearchTEMP(string autopartNumbers) //, int? maxItemCount)
        //{
        //    string[] autopartNumbersList = new string[] { };
        //    int maxItemCount = GetSearchLimit(); // get info from db about curren user search limit
        //    ViewBag.SearchLimit = maxItemCount;
        //    //var autoparts = (IQueryable<Part>)(from p in db.Parts
        //    //                 select new
        //    //                 {
        //    //                     p.Id,
        //    //                     p.ImportId,
        //    //                     p.Brand,
        //    //                     p.Number,
        //    //                     p.Name,
        //    //                     p.Details,
        //    //                     p.Size,
        //    //                     p.Weight,
        //    //                     p.Quantity,
        //    //                     p.Price,
        //    //                     p.Supplier,
        //    //                     p.DeliveryTime,
        //    //                     p.SupplierId
        //    //                 }
        //    //                 ).Take(0);

        //    //var autoparts = (from p in db.Parts
        //    //                 select p).Take(0);

        //    if (!String.IsNullOrEmpty(autopartNumbers))
        //    {
        //        // string txt = TextBox1.Text;
        //        string[] delimiters = { Environment.NewLine, ".", ",", ";", " " }; //new Char[] { '\n', '\r' }
        //        autopartNumbersList = autopartNumbers.Split(delimiters, StringSplitOptions.RemoveEmptyEntries); // StringSplitOptions.None

        //        var numbersList = new List<string>(autopartNumbersList);

        //        // two digits for search item message
        //        ViewBag.SearchLimit = maxItemCount;
        //        ViewBag.ItemsToSearch = numbersList.Count;

        //        if (maxItemCount < numbersList.Count)
        //        {
        //            numbersList.RemoveRange((int)maxItemCount, numbersList.Count() - (int)maxItemCount);
        //        }
        //        // find and add replacement to numbers list
        //        var autopartsReplacement = (from s in db.PartReplacement
        //                                    where numbersList.Contains(s.Number)
        //                                    select s.Replacement).ToList();
        //        numbersList.AddRange(autopartsReplacement);

        //        autopartNumbersList = numbersList.ToArray();


        //        //foreach (string autopartNumber in autopartNumbersList)
        //        //{
        //        //    if (!String.IsNullOrEmpty(autopartNumber))
        //        //    {
        //        //        autoparts = autoparts.Where(c => c.Number.Contains(autopartNumbersList));
        //        //    }
        //        //}


        //        //assemble an array of ID values
        //        //int[] customerIds = new int[] { 1, 2, 3 };
        //        //string autopartNumbersString = autopartNumbersList.ToString(", ");

        //        //autoparts = autoparts.Where(c => c.Number.Contains(autopartNumbersList));

        //        //autoparts = (from s in db.Parts
        //        //             where autopartNumbersList.Contains(s.Number)
        //        //             select s).Take(1000);

        //        var autoparts = (IEnumerable<AutoPartsWebSite.Models.Part>)(from p in db.Parts
        //                                                                    join a in db.PartAliases on p.Number equals a.Number into ps
        //                                                                    from a in ps.DefaultIfEmpty()
        //                                                                    where autopartNumbersList.Contains(p.Number)
        //                                                                    select new
        //                                                                    {
        //                                                                        p.Id,
        //                                                                        p.ImportId,
        //                                                                        p.Brand,
        //                                                                        p.Number,
        //                                                                        p.Name,
        //                                                                        p.Details,
        //                                                                        p.Size,
        //                                                                        p.Weight,
        //                                                                        p.Quantity,
        //                                                                        p.Price,
        //                                                                        p.Supplier,
        //                                                                        p.DeliveryTime,
        //                                                                        p.SupplierId
        //                                                                    }
        //                     ).Take(1000);

        //        foreach (Part part in autoparts)
        //        {
        //            part.Price = CalcUserPrice(part.Id);
        //            part.Quantity = CalcUserQuantity(part.Id);
        //        }
        //        //Session["AutopartsSearchResult"] = autoparts; //.ToList();
        //        Session["AutopartNumbersList"] = autopartNumbersList;
        //        return View(autoparts);
        //    }
        //    return View();
        //}


        //public ActionResult Excel()
        //{
        //    string[] autopartNumbersList = (string[])Session["AutopartNumbersList"];
        //    //var autoparts = (from s in db.Parts
        //    //                 where autopartNumbersList.Contains(s.Number)
        //    //                 select s).Take(1000);
        //    var autoparts = (from p in db.Parts
        //                     join a in db.PartAliases on p.Number equals a.Number into ps
        //                     from a in ps.DefaultIfEmpty()
        //                     where autopartNumbersList.Contains(p.Number)
        //                     //select p
        //                     select new
        //                     {
        //                         Id = p.Id,
        //                         ImportId = p.ImportId,
        //                         Brand = p.Brand,
        //                         Number = p.Number,
        //                         Name = !string.IsNullOrEmpty(a.Name) ? a.Name : p.Name,
        //                         Details = !string.IsNullOrEmpty(a.Details) ? a.Details : p.Details,
        //                         Size = !string.IsNullOrEmpty(a.Size) ? a.Size : p.Size,
        //                         Weight = !string.IsNullOrEmpty(a.Weight) ? a.Weight : p.Weight,
        //                         Quantity = p.Quantity,
        //                         Price = p.Price,
        //                         //Supplier = p.Supplier,
        //                         //DeliveryTime = p.DeliveryTime,
        //                         SupplierId = p.SupplierId
        //                     }
        //                         ).ToList()
        //                         .Select(x => new Part
        //                         {
        //                             Id = x.Id,
        //                             ImportId = x.ImportId,
        //                             Brand = x.Brand,
        //                             Number = x.Number,
        //                             Name = x.Name,
        //                             Details = x.Details,
        //                             Size = x.Size,
        //                             Weight = x.Weight,
        //                             Quantity = x.Quantity,
        //                             Price = x.Price,
        //                             //Supplier = x.Supplier,
        //                             //DeliveryTime = x.DeliveryTime,
        //                             SupplierId = x.SupplierId
        //                         }).Take(1000);

        //    foreach (Part part in autoparts)
        //    {
        //        part.Price = CalcUserPrice(part.Id);
        //        part.Quantity = CalcUserQuantity(part.Id);
        //    }
        //    using (ExcelPackage pck = new ExcelPackage())
        //    {
        //        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("ALFAPARTS-SearchResult");
        //        ws.Cells["A1"].LoadFromCollection(autoparts, true);
        //        // Загружаю коллекцию  "autoparts"
        //        // ToDo: еще нужно будет добавить русские хидеры
        //        Byte[] fileBytes = pck.GetAsByteArray();
        //        Response.Clear();
        //        Response.Buffer = true;
        //        Response.AddHeader("content-disposition", "attachment;filename=ALFAPARTS.xlsx");
        //        // Заменяю имя выходного Эксель файла

        //        Response.Charset = "";
        //        Response.ContentType = "application/vnd.ms-excel";
        //        StringWriter sw = new StringWriter();
        //        Response.BinaryWrite(fileBytes);
        //        Response.End();
        //    }

        //    return RedirectToAction("Index");
        //}

        public ActionResult Excel()
        {            
            var autoparts = (IEnumerable<Part>)Session["AutopartSearchResult"];

            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("ALFAPARTS-SearchResult");
                ws.Cells["A1"].LoadFromCollection(autoparts, true);
                // Загружаю коллекцию  "autoparts"
                // ToDo: еще нужно будет добавить русские хидеры
                Byte[] fileBytes = pck.GetAsByteArray();
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=ALFAPARTS.xlsx");
                // Заменяю имя выходного Эксель файла

                Response.Charset = "";
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                Response.BinaryWrite(fileBytes);
                Response.End();
            }

            return RedirectToAction("Index");
        }


        private int GetSearchLimit()
        {
            string currentUserId = User.Identity.GetUserId();
            ApplicationUserManager UserManager = HttpContext.GetOwinContext()
                                           .GetUserManager<ApplicationUserManager>();
            var user = UserManager.FindById(currentUserId);
            if (user == null)
            {
                return 1;
            }
            return user.SearchLimit;
        }

        private string CalcUserPrice(int PartId)
        {
            decimal defaultRate = 10;

            // Random rnd = new Random();
            Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            decimal anRegistredUserRate = rnd.Next(5, 8); // issue #24


            Part part = db.Parts.Find(PartId);
            if (part == null) // if part not exists - return 0
            {
                return "0";
            }

            Supplier supplier = db.Suppliers.Find(part.SupplierId);
            if (supplier == null)  // if supplier not exists - return price + defaultRate
            {
                return ((100 + defaultRate) * Convert.ToDecimal(part.Price)/100).ToString();
            }

            string currentUserId = User.Identity.GetUserId();
            ApplicationUserManager UserManager = HttpContext.GetOwinContext()
                                           .GetUserManager<ApplicationUserManager>();
            var user = UserManager.FindById(currentUserId);
            if (user == null)  // if not defined user - return price + SuppliersRate
            {
                // return ((100 + supplier.Rate) * Convert.ToDecimal(part.Price) / 100).ToString();
                return ((100 + anRegistredUserRate) * Convert.ToDecimal(part.Price) / 100).ToString(); 
            }

            var rate = (from r in db.Rates
                where r.UserId.Equals(currentUserId) && r.SupplierId.Equals(part.SupplierId) 
                select r).FirstOrDefault();
            if (rate == null)  // if rate not exists - return price +  SuppliersRate
            {
                return ((100 + supplier.Rate) * Convert.ToDecimal(part.Price) / 100).ToString();
            }

            return ((100 + rate.Value) * Convert.ToDecimal(part.Price) / 100).ToString();
        }

        private string CalcUserQuantity(int PartId)
        {
            Part part = db.Parts.Find(PartId);
            if (part == null) // if part not exists - return 0
            {
                return "0";
            }
            if (Convert.ToInt32(part.Quantity) > 10)
            {
                int operand1 = Convert.ToInt32(part.Quantity);
                int operand2 = Convert.ToInt32(System.Math.Pow(10, (part.Quantity.Length - 1)));                
                return ((operand1 / operand2) * operand2).ToString();
            }
            if (Convert.ToInt32(part.Quantity) > 5)
            {
                return "5";
            }
            if (Convert.ToInt32(part.Quantity) <= 5)
            {
                return part.Quantity;
            }
            return "0"; //something going wrong
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
