﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HelpDesk.Models;
using Microsoft.AspNet.Identity;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class ZgloszeniasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Zgloszenias
        public ActionResult Index()
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            if (user.ChangedPassword == 1)
            {
                if (user.KategorieId == 8)
                {
                    var zgloszenias = db.Zgloszenias.Include(z => z.Kategorie).Include(z => z.Statusy).Where(z => z.Uzytkownik == User.Identity.Name);
                    return View(zgloszenias.ToList());

                }
                else if (user.KategorieId == 9)
                {
                    var zgloszenias = db.Zgloszenias.Include(z => z.Kategorie).Include(z => z.Statusy);
                    return View(zgloszenias.ToList());
                }
                else
                {
                    var zgloszenias = db.Zgloszenias.Include(z => z.Kategorie).Include(z => z.Statusy).Where(z => z.KategorieId == user.KategorieId || z.Uzytkownik == User.Identity.Name);
                    return View(zgloszenias.ToList());
                }
            }
            else
            {
                return RedirectToAction("ChangePassword", "Manage");
            }
            
        }

        // GET: Zgloszenias/Details/5
        public ActionResult Details(int? id)
        {
            var model = new ZgloszeniaViewModels();
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            Zgloszenia zgl = db.Zgloszenias.Find(id);
            if (user.ChangedPassword == 1)
            {
                if (id == null || user.KategorieId == zgl.KategorieId || user.UserName == zgl.Uzytkownik || user.KategorieId == 9)
                {
                    if (zgl == null)
                    {
                        return HttpNotFound();
                    }
                    Kategorie a = db.Kategories.Find(zgl.KategorieId);
                    ViewBag.Kategoria = a.Nazwa;
                    Statusy b = db.Statusys.Find(zgl.StatusyId);
                    ViewBag.Status = b.Nazwa;
                    ViewBag.edycja = 0;
                    ViewBag.adm = 0;

                    if (user.KategorieId == zgl.KategorieId)
                    {
                        ViewBag.edycja = 1;
                    }

                    if (user.KategorieId == 9)
                    {
                        ViewBag.edycja = 1;
                        ViewBag.adm = 1;
                    }


                    model.Wiadomosci = db.Wiadomoscis.Where(z => z.ZgloszeniaId == id).ToList();
                    model.Zgloszenia = zgl;
                    model.idwiad = (int)id;
                    model.nad = user.UserName;
                    return View(model);

                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

            }
            else
            {
                return RedirectToAction("ChangePassword", "Manage");

            }
        }

        [HttpPost]
        public ActionResult Details (ZgloszeniaViewModels model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var wiad = new Wiadomosci();
                    wiad.DataDodania = DateTime.Now;
                    wiad.ZgloszeniaId = model.idwiad;
                    wiad.Nadawca = model.nad;
                    wiad.Tresc = model.Wiad;
                    db.Wiadomoscis.Add(wiad);
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = model.idwiad });
                }
                catch (Exception)
                {

                }
            }
            return RedirectToAction("Index");
        }

        // GET: Zgloszenias/Create
        public ActionResult Create()
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            if (user.ChangedPassword == 1)
            {
                ViewBag.KategorieId = new SelectList(db.Kategories.Where(z => z.Id < 8), "Id", "Nazwa");
                ViewBag.StatusyId = new SelectList(db.Statusys, "Id", "Nazwa");
                return View();
            }
            else
            {
                return RedirectToAction("ChangePassword", "Manage");
            }
        }

        // POST: Zgloszenias/Create
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nazwa,Opis,Komentarz,StatusyId,KategorieId,Uzytkownik")] Zgloszenia zgloszenia)
        {
            if (ModelState.IsValid)
            {
                zgloszenia.StatusyId = 1;
                zgloszenia.Uzytkownik = User.Identity.Name;
                zgloszenia.Komentarz = "";
                zgloszenia.DataDodania = DateTime.Now;
                db.Zgloszenias.Add(zgloszenia);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.KategorieId = new SelectList(db.Kategories, "Id", "Nazwa", zgloszenia.KategorieId);
            ViewBag.StatusyId = new SelectList(db.Statusys, "Id", "Nazwa", zgloszenia.StatusyId);
            return View(zgloszenia);
        }

        // GET: Zgloszenias/Edit/5
        public ActionResult Edit(int? id)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            Zgloszenia zgl = db.Zgloszenias.Find(id);

            if (user.ChangedPassword == 1)
            {
                if (id == null || user.KategorieId == zgl.KategorieId || user.KategorieId == 9)
                {
                    if (zgl == null)
                    {
                        return HttpNotFound();
                    }
                    ViewBag.KategorieId = new SelectList(db.Kategories, "Id", "Nazwa", zgl.KategorieId);
                    ViewBag.StatusyId = new SelectList(db.Statusys, "Id", "Nazwa", zgl.StatusyId);
                    return View(zgl);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                }
                
            }
            else
            {
                return RedirectToAction("ChangePassword", "Manage");

            }
        }

        // POST: Zgloszenias/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nazwa,Opis,Komentarz,StatusyId,KategorieId,Uzytkownik,DataDodania")] Zgloszenia zgloszenia)
        {
            Zgloszenia stary_wpis = db.Zgloszenias.Find(zgloszenia.Id);

            if (ModelState.IsValid)
            {
                stary_wpis.Nazwa = zgloszenia.Nazwa;
                stary_wpis.Opis = zgloszenia.Opis;
                stary_wpis.Komentarz = zgloszenia.Komentarz;
                stary_wpis.StatusyId = zgloszenia.StatusyId;
                stary_wpis.KategorieId = zgloszenia.KategorieId;


                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.KategorieId = new SelectList(db.Kategories, "Id", "Nazwa", zgloszenia.KategorieId);
            ViewBag.StatusyId = new SelectList(db.Statusys, "Id", "Nazwa", zgloszenia.StatusyId);
            return View(zgloszenia);
        }

        // GET: Zgloszenias/Delete/5
        public ActionResult Delete(int? id)
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            if (user.ChangedPassword == 1)
            {
                if (id == null || user.KategorieId != 9)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Zgloszenia zgloszenia = db.Zgloszenias.Find(id);
                if (zgloszenia == null)
                {
                    return HttpNotFound();
                }
                Kategorie a = db.Kategories.Find(zgloszenia.KategorieId);
                ViewBag.Kategoria = a.Nazwa;
                Statusy b = db.Statusys.Find(zgloszenia.StatusyId);
                ViewBag.Status = b.Nazwa;
                return View(zgloszenia);
            }
            else
            {
                return RedirectToAction("ChangePassword", "Manage");
            }
            
        }

        // POST: Zgloszenias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Zgloszenia zgloszenia = db.Zgloszenias.Find(id);
            int x = db.Wiadomoscis.Where(m => m.ZgloszeniaId == id).Count();
            for (int i = 0; i < x; i++)
            {
                Wiadomosci wiad = db.Wiadomoscis.Where(m => m.ZgloszeniaId == id).FirstOrDefault();
                db.Wiadomoscis.Remove(wiad);
                wiad = null;
            }
            db.Zgloszenias.Remove(zgloszenia);
            db.SaveChanges();
            return RedirectToAction("Index");
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
