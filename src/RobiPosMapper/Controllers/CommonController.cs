using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RobiPosMapper.Models;
using System.IO;
using ZXing;
using ZXing.Rendering;
using System.Drawing;

namespace RobiPosMapper.Controllers
{
    public class CommonController : Controller
    {
        [HttpPost]
        public JsonResult GetAllRegions()
        {
            try
            {
                List<Models.Region> regions = RegionManager.GetAllRegions();
                return Json(new { status = "success", data = regions }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(new { status = "error", data = "" }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult GetRegionSpecificAreas(Int32 regionId)
        {
            try
            {
                List<Area> areas = AreaManager.RegionSpecificAreaList(regionId);
                return Json(new { status = "success", data = areas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(new { status = "error", data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetAreaSpecificThanas(Int32 areaId)
        {
            try
            {
                List<Thana> thanas = ThanaManager.AreaSpecificThanaList(areaId);
                return Json(new { status = "success", data = thanas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(new { status = "error", data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetThanaSpecificWards(Int32 thanaId)
        {
            try
            {
                List<Ward> wards = WardManager.ThanaSpecificWardList(thanaId);
                return Json(new { status = "success", data = wards }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(new { status = "error", data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetWardSpecificMauzas(Int32 wardId)
        {
            try
            {
                List<Mauza> mauzas = MauzaManager.WardSpecificMauzaList(wardId);
                return Json(new { status = "success", data = mauzas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(new { status = "error", data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetMauzaSpecificVillages(Int32 mauzaId)
        {
            try
            {
                List<Village> villages = VillageManager.MauzaSpecificVillageList(mauzaId);
                return Json(new { status = "success", data = villages }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(new { status = "error", data = "" }, JsonRequestBehavior.AllowGet);
            }
        }


        //QR Code Reader
        [HttpPost]
        public JsonResult ReadQrCode(FormCollection data)
        {
            Byte[] imagefile;
            String imageName = String.Empty;
            Int32 DecodingStatus = 0;
            String decodedData = String.Empty;

           

            if (Request.Files["QrImage"] != null)
            {
                imageName = Guid.NewGuid().ToString() + ".png";
                var directory = HttpContext.Server.MapPath("~/Photos");
                string imagePath = Path.Combine(directory, "SubmittedQr", imageName);
                FileStream fs = new FileStream(imagePath, FileMode.CreateNew);

                using (var binaryReader = new BinaryReader(Request.Files["QrImage"].InputStream))
                {
                    imagefile = binaryReader.ReadBytes(Request.Files["QrImage"].ContentLength);//image
                }


                try
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(imagefile);
                    bw.Close();
                }
                catch (Exception)
                {
                    //TODO
                }

                Bitmap bitmap = new Bitmap(imagePath);
                try
                {
                    BarcodeReader reader = new BarcodeReader { AutoRotate = true };
                    reader.Options.TryHarder = true;

                    Result result = reader.Decode(bitmap);
                    if (result==null)
                    {
                        DecodingStatus = 3; //does not contain qr code
                    }
                    else
                    {
                        decodedData = result.Text;
                        DecodingStatus = 1;
                    }
                }
                catch
                {
                    DecodingStatus = 4; //error in decoding
                }
            }
            else
            {
                DecodingStatus = 2; //image is null
            }

            return Json(new{status=DecodingStatus,qrdata=decodedData} ,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetAreaSpecificRsps(Int32 areaId)
        {
            try
            {
                List<Rsp> Rsps = RspManager.AreaSpecificRspList(areaId);
                return Json(new { status = "success", data = Rsps }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(new { status = "error", data = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}