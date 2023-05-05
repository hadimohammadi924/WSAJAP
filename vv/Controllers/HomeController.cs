using Sch_WCFApplication;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using vv.Models;

namespace vv.Controllers
{
    public class HomeController : Controller
    {

        pgtabir1_shineEntities9 db = new pgtabir1_shineEntities9();
        pgtabir1_shineEntities9 dbb = new pgtabir1_shineEntities9();



        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }





        public async Task<JsonResult> smsssAsync(String Mobile, String Code)
        {


            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", "ezu2aHdrqf4PTt7PSqOCiQymY0zhkBF5uEcltOSsdWHGhXOCxsJhsaSSPOfLUcZz");

            var payload = @"{" + @" ""mobile"":""" + Mobile.ToString() + @"""," + @" ""templateId"": 513845," + @" ""parameters"": [" + @"{" + @" ""name"": ""CODE""," + @" ""value"": """ + Code.ToString() + @"""" + @"}" + @"]" + @"}";


            // var payloadd = @" "mobile" = x1,@" "templateId" = "513845";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.sms.ir/v1/send/verify", content);
            var result = await response.Content.ReadAsStringAsync();

            return Json(new { result }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult Login(String Phone)
        {
            var temp = db.Users.FirstOrDefault(p => p.Users_Phone == Phone);
            String Code;
            if (temp == null)
            {
                Users user = new Users();
                user.Users_Phone = Phone;
                user.Users_Insert_Date = DateTime.Now.ToString();
                user.Users_LastSeen = DateTime.Now.ToString();
                   Random r = new Random();
                Code = r.Next(1000, 9999).ToString();
                user.Users_Code = Code;
                db.Users.Add(user);
                db.SaveChanges();
                smsssAsync(Phone, Code);
            }
            else
            {
                Random r = new Random();
                Code = r.Next(1000, 9999).ToString();
                temp.Users_Code = Code;
                temp.Users_Insert_Date = DateTime.Now.ToString();
                temp.Users_LastSeen = DateTime.Now.ToString();

                db.Entry(temp).State = EntityState.Modified;
                db.SaveChanges();
                smsssAsync(Phone, Code);
            }
            
           

            return Json(new { statue = "Success" }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult checkCode(String Phone, String Code)
        {
            var temp = db.Users.FirstOrDefault(p => p.Users_Phone == Phone);
            if (temp.Users_Code == Code)
            {
                TimeSpan tempTime = DateTime.Now - Convert.ToDateTime( temp.Users_Insert_Date);
                double second = tempTime.TotalSeconds;
                if (second > 60)
                {
                    return Json(new { statue = "CodeExpired" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { statue = "Success", id = temp.Users_ID, name = temp.Users_Name }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { statue = "InvalidCode" }, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult getChats(int Users_ID)
        {
            var list = db.GroupsMember.Where(p => p.GroupsMember_Member_ID == Users_ID).ToList();
            List<Groups> groups = new List<Groups>();
            foreach (var item in list)
            {
                var group = db.Groups.FirstOrDefault(p => p.Groups_ID == item.GroupsMember_Group_ID);
                groups.Add(group);
            }

            var items = groups.Select(s => new
            {
                Groups_ID = s.Groups_ID,
                Groups_Title = s.Groups_Title,
                Groups_Username = s.Groups_Username,
                Groups_Avatar = s.Groups_Avatar,
                Groups_About = s.Groups_About,
            });
            return Json(new { items }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getMessages(int Group_ID)
        {
            var list = db.GroupMessages.Where(p => p.GroupMessages_Group_ID == Group_ID).ToList();
            var items = list.Select(s => new
            {
                GroupMessages_ID = s.GroupMessages_ID,
                GroupMessages_Insert_Date = s.GroupMessages_Insert_Date,
                GroupMessages_Sender_ID = s.GroupMessages_Sender_ID,
                GroupMessages_Text = s.GroupMessages_Text,
                GroupMessages_File = s.GroupMessages_File,
                UserImage = s.Users.Users_Avatar,
                UserName = s.Users.Users_Name
            });
            return Json(new { items }, JsonRequestBehavior.AllowGet);
        }


        public void sendMessageToGroup(String Text, int SenderID, int Group_ID)
        {
            GroupMessages message = new GroupMessages();
            message.GroupMessages_Group_ID = Group_ID;
            message.GroupMessages_Insert_Date = DateTime.Now.ToString();
            message.GroupMessages_Sender_ID = SenderID;
            message.GroupMessages_Text = Text;
            db.GroupMessages.Add(message);
            db.SaveChanges();

            String GroupName = db.Groups.FirstOrDefault(p => p.Groups_ID == Group_ID).Groups_Title;
            String SenderName = db.Users.FirstOrDefault(p => p.Users_ID == SenderID).Users_Name;

            var grpMembers = db.GroupsMember.Where(p => p.GroupsMember_Group_ID == Group_ID).ToList();
            foreach (var item in grpMembers)
            {
                if (item.GroupsMember_Member_ID != SenderID)
                {
                    int usrID = item.GroupsMember_Member_ID;
                    var devices = db.Devices.FirstOrDefault(p => p.Devices_Users_ID == usrID);
                    String tok = devices.Devices_Token;
                    notification(tok, GroupName, SenderName + " : " + Text);
                    // PushNotification pushNotification = new PushNotification(tok, Group_ID, ToString(), message.Users.Users_Name + ":" + Text);
                }
            }
        }
        public void sendMessageToGroupp(String FilePatch, String Text, int SenderID, int Group_ID)
        {
            GroupMessages message = new GroupMessages();
            message.GroupMessages_Group_ID = Group_ID;
            message.GroupMessages_Insert_Date = DateTime.Now.ToString();
            message.GroupMessages_Sender_ID = SenderID;
            message.GroupMessages_File = FilePatch;
            message.GroupMessages_Text = Text;
            db.GroupMessages.Add(message);
            db.SaveChanges();

            String GroupName = db.Groups.FirstOrDefault(p => p.Groups_ID == Group_ID).Groups_Title;
            String SenderName = db.Users.FirstOrDefault(p => p.Users_ID == SenderID).Users_Name;

            var grpMembers = db.GroupsMember.Where(p => p.GroupsMember_Group_ID == Group_ID).ToList();
            foreach (var item in grpMembers)
            {
                if (item.GroupsMember_Member_ID != SenderID)
                {
                    int usrID = item.GroupsMember_Member_ID;
                    var devices = db.Devices.FirstOrDefault(p => p.Devices_Users_ID == usrID);
                    String tok = devices.Devices_Token;
                    notification(tok, GroupName, SenderName + " : " + Text);
                    // PushNotification pushNotification = new PushNotification(tok, Group_ID, ToString(), message.Users.Users_Name + ":" + Text);
                }
            }
        }

        public void addDevice(int Users_ID, String DeviceModel, String Token, String DeviceID)
        {
            var temp = db.Devices.FirstOrDefault(p => p.Devices_Users_ID == Users_ID);
            if (temp != null)
            {
                temp.Devices_Users_ID = Users_ID;
                temp.Devices_Token = Token;
                temp.Devices_Model = DeviceModel;
                temp.Devices_DeviceID = DeviceID;
                db.Entry(temp).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                Devices device = new Devices();
                device.Devices_Users_ID = Users_ID;
                device.Devices_Token = Token;
                device.Devices_Model = DeviceModel;
                device.Devices_DeviceID = DeviceID;
                db.Devices.Add(device);
                db.SaveChanges();
            }

        }

        public void notification(String DeviceToken, String Title, String Text)
        {
            PushNotification pushNotification = new PushNotification();
            pushNotification.SendNotification(DeviceToken, Title, Text);
        }


        [HttpPost]
        public JsonResult syncContacts(string numbers, string names)
        {
            List<ContactsViewModel> list = new List<ContactsViewModel>();
            List<String> numbersList = numbers.Split(',').ToList();
            List<String> namesList = names.Split(',').ToList();
            for (int i = 0; i < numbersList.Count; i++)
            {
                String number = getNumberLast(numbersList[i]);
                var item = db.Users.FirstOrDefault(p => DbFunctions.Like(p.Users_Phone, "%" + number));
                if (item == null)
                {
                    ContactsViewModel temp = new ContactsViewModel();
                    temp.Name = namesList[i];
                    temp.Number = numbersList[i];
                    temp.inAppStatus = false;
                    
                    list.Add(temp);
                }
                else
                {
                    ContactsViewModel temp = new ContactsViewModel();
                    temp.Name = item.Users_Name;
                    temp.Number = item.Users_Phone;
                    temp.Avatar = item.Users_Avatar;
                    temp.id = item.Users_ID;
                    temp.LastSeen = item.Users_LastSeen.ToString();
                    temp.inAppStatus = true;
                    list.Add(temp);
                }
            }

            return Json(new { list = list.OrderByDescending(p => p.inAppStatus) }, JsonRequestBehavior.AllowGet);

        }

        public String getNumberLast(String number)
        {
            try
            {
                char[] charArray = number.ToCharArray();
                Array.Reverse(charArray);
                String reverse = new String(charArray);

                reverse = reverse.Substring(0, 10);
                charArray = reverse.ToCharArray();
                Array.Reverse(charArray);
                String num = new String(charArray);
                return num;
            }
            catch
            {
                return number;
            }

        }


        public JsonResult completeUser(int Users_ID,String Users_Name,String Users_Username, String Users_Bio,String Users_Avatar)
        {
            var temp = db.Users.FirstOrDefault(p => p.Users_ID == Users_ID);
           // String Code;
            if (temp == null)
            {

                return Json(new { statue = "Fail" }, JsonRequestBehavior.AllowGet);
            }
            else
            {

               
                temp.Users_Name = Users_Name;
                temp.Users_Username = Users_Username;
                temp.Users_Bio = Users_Bio;
                temp.Users_Avatar = Users_Avatar;
                db.Entry(temp).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { statue = "Success" }, JsonRequestBehavior.AllowGet);

            }

                                 

        }





        [HttpPost]
        public JsonResult UploadFile(HttpPostedFileBase file, int SenderID, int Group_ID)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = HttpContext.Server.MapPath(@"~/msgPIC");

                    bool exists = System.IO.Directory.Exists(path);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);
                    string extension = Path.GetExtension(file.FileName);
                    string newFileName = Guid.NewGuid() + extension;
                    string filePath = Path.Combine(path, newFileName);
                    file.SaveAs(filePath);

                    sendMessageToGroupp("https://pgtab.ir/msgPIC/" + newFileName, "", SenderID, Group_ID);

                    return Json(new { Status = "OK", Path = "https://pgtab.ir/msgPIC/" + newFileName }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { Status = ex.Data }, JsonRequestBehavior.AllowGet);
                }
            else
            {
                return Json(new { Status = "No File" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult upload_AVATAR(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/AVATAR"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();
        }
        public ActionResult upload_msgPIC(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/msgPIC"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();
        }
        public ActionResult upload_chatfile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/chatfile"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();
        }


        public JsonResult CreateChat(string id, string idd)
        {
            var list = db.Groups.Where(p => p.Groups_Title == id || p.Groups_Title == idd).ToList();
            var listt = list.Where(p => p.Groups_About == id || p.Groups_About == idd).ToList();

            var items = listt.Select(s => new
            {
                Groups_ID = s.Groups_ID,
                Groups_Title = s.Groups_Title,
                Groups_Username = s.Groups_Username,
                Groups_Avatar = s.Groups_Avatar,
                Groups_About = s.Groups_About,
            });

            if (listt.Count == 0) {
                Groups groups = new Groups();
                groups.Groups_Creator_ID = Convert.ToInt32(id);
                groups.Groups_Title = idd.ToString();
                groups.Groups_About = id.ToString();
                groups.Groups_Avatar = "1";
                groups.Groups_Insert_Date = DateTime.Now.ToString();
                groups.Groups_Type = 1;
                groups.Groups_Username = "PrivateChat";
                groups.Groups_Status = true;
                db.Groups.Add(groups);
                db.SaveChanges();

                GroupsMember member = new GroupsMember();
                member.GroupsMember_Group_ID = groups.Groups_ID;
                member.GroupsMember_Member_ID = Convert.ToInt32(id);
                member.GroupsMember_Admin_Status = true;
                member.GroupsMember_Blocked_Status = true;
                db.GroupsMember.Add(member);
                db.SaveChanges();
                GroupsMember memberr = new GroupsMember();
                memberr.GroupsMember_Group_ID = groups.Groups_ID;
                memberr.GroupsMember_Member_ID = Convert.ToInt32(idd);
                memberr.GroupsMember_Admin_Status = true;
                memberr.GroupsMember_Blocked_Status = true;
                db.GroupsMember.Add(memberr);
                db.SaveChanges();
                var listd = db.Groups.Where(p => p.Groups_Title == id || p.Groups_Title == idd).ToList();
                var listtd = listd.Where(p => p.Groups_About == id || p.Groups_About == idd).ToList();

                var itemsd = listtd.Select(s => new
                {
                    Groups_ID = s.Groups_ID,
                    Groups_Title = s.Groups_Title,
                    Groups_Username = s.Groups_Username,
                    Groups_Avatar = s.Groups_Avatar,
                    Groups_About = s.Groups_About,
                });




                return Json(itemsd, JsonRequestBehavior.AllowGet);
            }
            else
            { return Json(items, JsonRequestBehavior.AllowGet);
            }
            

              

             


        }
        public JsonResult userd(int userid) {
            Users hadi = new Users();
            var list = db.Users.Where(p => p.Users_ID == userid ).ToList();
            var items = list.Select(s => new
            {
                User_ID = s.Users_ID,
                Users_Name = s.Users_Name,
                Users_Username = s.Users_Username,
                Users_Phone = s.Users_Phone,
                Users_LastSeen = s.Users_LastSeen,
                Users_Avatar = s.Users_Avatar,
               
            });
            

            if (list.Count ==0)
            {
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(items, JsonRequestBehavior.AllowGet);
            }
           



        }


        ////////////////////////////////SAJAP////////////////////////////////

        public async Task<JsonResult> SsmsssAsync(String Mobile, String Code)
        {


            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-api-key", "ezu2aHdrqf4PTt7PSqOCiQymY0zhkBF5uEcltOSsdWHGhXOCxsJhsaSSPOfLUcZz");

            var payload = @"{" + @" ""mobile"":""" + Mobile.ToString() + @"""," + @" ""templateId"": 803314," + @" ""parameters"": [" + @"{" + @" ""name"": ""CODE""," + @" ""value"": """ + Code.ToString() + @"""" + @"}" + @"]" + @"}";


            // var payloadd = @" "mobile" = x1,@" "templateId" = "513845";
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.sms.ir/v1/send/verify", content);
            var result = await response.Content.ReadAsStringAsync();

            return Json(new { result }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult SLogin(int personalcode)
        {
            var temp = dbb.SUSERS.FirstOrDefault(p => p.personalcode == personalcode);
            String Code;
            if (temp == null)
            {
                //  Users user = new Users();
                //  user.Users_Phone = Phone;
                //   user.Users_Insert_Date = DateTime.Now.ToString();
                //   user.Users_LastSeen = DateTime.Now.ToString();
                //    Random r = new Random();
                //    Code = r.Next(1000, 9999).ToString();
                //    user.Users_Code = Code;
                //     db.Users.Add(user);
                //    db.SaveChanges();
                // smsssAsync(Phone, Code);
                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Random r = new Random();
                Code = r.Next(1000, 9999).ToString();
                temp.x2 = Code;
                temp.x1 = DateTime.Now.ToString();
                //   temp.Users_LastSeen = DateTime.Now.ToString();
                string Phone = temp.phone;
                dbb.Entry(temp).State = EntityState.Modified;
                dbb.SaveChanges();
                SsmsssAsync(Phone, Code);
            }



            return Json(new { statue = "Success" }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult ScheckCode(int personalcode, String Code)
        {
            var temp = dbb.SUSERS.FirstOrDefault(p => p.personalcode == personalcode);
            if (temp.x2 == Code)
            {
                TimeSpan tempTime = DateTime.Now - Convert.ToDateTime(temp.x1);
                double second = tempTime.TotalSeconds;
                if (second > 60)
                {
                    return Json(new { statue = "CodeExpired" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { statue = "Success", id = temp.personalcode, name = temp.name,fname=temp.fname,phone=temp.phone,pic=temp.pic, }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { statue = "InvalidCode" }, JsonRequestBehavior.AllowGet);

            }
        }
       
        public JsonResult SUSERS(int personalcode)
        {
            var temp = dbb.SUSERS.FirstOrDefault(p => p.personalcode == personalcode);
            if (temp == null)
            {
                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);
            }
            else { 
             
                    return Json(new { statue = "Success", personalcode = temp.personalcode, name = temp.name, fname = temp.fname, phone = temp.phone, pic = temp.pic,x3=temp.x3, x4 = temp.x4, x5 = temp.x5 }, JsonRequestBehavior.AllowGet);
          
              }

            }
        public JsonResult SDATA(int d_personalcode)
        {
            var temp = dbb.SDATA.FirstOrDefault(p => p.d_personalcode == d_personalcode);
            if (temp == null)
            {
                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(new { statue = "Success", d_personalcode = temp.d_personalcode, name = temp.name, fname = temp.fname, codemelli = temp.codemelli, shenasname = temp.shenasname, tell = temp.tell, mobile = temp.mobile, madrak = temp.madrak, reshtetahsili = temp.reshtetahsili, gerayesh = temp.gerayesh, tozihat = temp.tozihat, noegarardad = temp.noegarardad, mahalesodor = temp.mahalesodor, saletavalode = temp.saletavalode, mahetavalode = temp.mahetavalode, rozetavalod = temp.rozetavalod, address = temp.address, jensiyat = temp.jensiyat, dadname = temp.dadname, shomarehesab = temp.shomarehesab, shaba = temp.shaba, bank = temp.bank, vahed = temp.vahed, shomarebime = temp.shomarebime, tarikheestekhdam = temp.tarikheestekhdam, shobe = temp.shobe, zirshobe = temp.zirshobe, sath = temp.sath, safesetade = temp.safesetade, d1 = temp.d1, d2 = temp.d2, d3 = temp.d3, d4 = temp.d4, xx = temp.d1, d5 = temp.d5, d6 = temp.d6, d7 = temp.d7, d8 = temp.d8, d9 = temp.d9, d10 = temp.d10, d11 = temp.d11, d12 = temp.d12, d13 = temp.d13, d14 = temp.d14, d15 = temp.d15 }, JsonRequestBehavior.AllowGet);

            }

        }
        public JsonResult Ahkam(int userpersonal)
        {
            var temp = dbb.ahkam.FirstOrDefault(p => p.userpersonal == userpersonal);
            if (temp == null)
            {
                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(new { statue = "Success", userpersonal = temp.userpersonal, type = temp.type, date = temp.date, type1 = temp.type1, type2 = temp.type2, type3 = temp.type3, type4 = temp.type4, type5 = temp.type5 }, JsonRequestBehavior.AllowGet);

            }

        }
        public JsonResult Shenasname(int userpersonal)
        {
            var temp = dbb.shenasname.FirstOrDefault(p => p.userpersonal == userpersonal);
            if (temp == null)
            {
                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(new { statue = "Success", userpersonal = temp.userpersonal, type = temp.type, date = temp.date, type1 = temp.type1, type2 = temp.type2, type3 = temp.type3, type4 = temp.type4, type5 = temp.type5 }, JsonRequestBehavior.AllowGet);

            }

        }
        public JsonResult Tanbih(int userpersonal)
        {
            var temp = dbb.tanbih.FirstOrDefault(p => p.userpersonal == userpersonal);
            if (temp == null)
            {
                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(new { statue = "Success", userpersonal = temp.userpersonal, type = temp.type, date = temp.date, type1 = temp.type1, type2 = temp.type2, type3 = temp.type3, type4 = temp.type4, type5 = temp.type5 }, JsonRequestBehavior.AllowGet);

            }

        }
        public JsonResult Tarfi(int userpersonal)
        {
            var temp = dbb.tarfi.FirstOrDefault(p => p.userpersonal == userpersonal);
            if (temp == null)
            {
                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(new { statue = "Success", userpersonal = temp.userpersonal, type = temp.type, date = temp.date, type1 = temp.type1, type2 = temp.type2, type3 = temp.type3, type4 = temp.type4, type5 = temp.type5 }, JsonRequestBehavior.AllowGet);

            }

        }
        public JsonResult Zemanatname(int userpersonal)
        {
            var temp = dbb.zemanatname.FirstOrDefault(p => p.userpersonal == userpersonal);
            if (temp == null)
            {
                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(new { statue = "Success", userpersonal = temp.userpersonal, type = temp.type, date = temp.date, type1 = temp.type1, type2 = temp.type2, type3 = temp.type3, type4 = temp.type4, type5 = temp.type5 }, JsonRequestBehavior.AllowGet);

            }

        }

        public JsonResult PMP(string sendern,string text,string date,string time,string uu,string x1,string x2,string x3)
        {
            pmp Spmp = new pmp();
            Spmp.sendern = sendern;
            Spmp.text = text;
            Spmp.date = date;
            Spmp.time = time;
            Spmp.uu = uu;
            Spmp.x1 = x1;
            Spmp.x2 = x2;
            Spmp.x3 = x3;
           
            db.pmp.Add(Spmp);
            db.SaveChanges();
            return Json(new { statue = "ok" }, JsonRequestBehavior.AllowGet);
        }



        //////first cumment
        ////seced comment

        public JsonResult insert_tiket(tiket tik)
        {
            dbb.tiket.Add(tik);
            dbb.SaveChanges();

            tiket tt = dbb.tiket.ToList().Last();


            var jsondata = Json(tt, JsonRequestBehavior.AllowGet);
            return jsondata;
        }

        public String insert_respone(int id_tiket, string tdate, string ttime, string tcategori, string ttitle, string tdescription, string tbgcode, string bgname, string btell, string tvisitor, string tresponse, string trdate, string trtime, string truser, string tstatus, string x1, string x2, string x3, string x4)
        {
            tiket selectcust = dbb.tiket.Find(id_tiket);
            if (selectcust != null)
            {
                selectcust.tdate = tdate;
                selectcust.ttime = ttime;
                selectcust.tcategori = tcategori;
                selectcust.ttitle = ttitle;
                selectcust.tdescription = tdescription;
                selectcust.tpgcode = tbgcode;
                selectcust.tgname = bgname;
                selectcust.btell = btell;
                selectcust.tvisitor = tvisitor;
                selectcust.tresponse = tresponse;
                selectcust.trdate = trdate;
                selectcust.trtime = trtime;
                selectcust.truser = truser;
                selectcust.tstatuse = tstatus;
                selectcust.x1 = x1;
                selectcust.x2 = x2;
                selectcust.x3 = x3;
                selectcust.x4 = x4;

                dbb.Entry(selectcust).State = System.Data.Entity.EntityState.Modified;
                dbb.SaveChanges();
                return "تغیرات لحاظ شد";

            }
            else
            {
                return "نتوانستیم تغیرات را اعمال کنیم";
            }
        }
        public JsonResult getaltiket()
        {
            List<tiket> list;
            list = dbb.tiket.ToList();
            var jsondata = Json(list, JsonRequestBehavior.AllowGet);
            return jsondata;
        }
        public JsonResult getaltikett(int id_tiket)
        {
            tiket selecttiket = dbb.tiket.Find(id_tiket);
            // List<tiket> list;
            //  list = mode8.tiket.ToList();
            var jsondata = Json(selecttiket, JsonRequestBehavior.AllowGet);
            return jsondata;
        }
     
        public JsonResult update_lastupdate(int suserid,string lastupdate,string Devise_name,string Devise_ID)
        {
            SUSERS selectrecord = dbb.SUSERS.Find(suserid);
            if (selectrecord != null)
            {
                selectrecord.lastupdate = lastupdate;
                selectrecord.Devise_name = Devise_name;
                selectrecord.Devise_ID = Devise_ID;
               


                dbb.Entry(selectrecord).State = System.Data.Entity.EntityState.Modified;
                dbb.SaveChanges();
             
               return Json(new { statue = "ok" }, JsonRequestBehavior.AllowGet);

            }
            else
            {

                return Json(new { statue = "no" }, JsonRequestBehavior.AllowGet);

            }
        }



    }
}