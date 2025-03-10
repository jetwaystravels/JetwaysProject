﻿using System;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Nancy.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using Sessionmanager;
using Bookingmanager_;
using System.ComponentModel;
using Utility;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OnionConsumeWebAPI.Extensions;
using OnionConsumeWebAPI.Controllers.Indigo;
using OnionArchitectureAPI.Services.Indigo;
using OnionArchitectureAPI.Services.Spicejet;
using IndigoSessionmanager_;
using System.Diagnostics.Metrics;
using System.Globalization;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using OnionConsumeWebAPI.ApiService;
using OnionArchitectureAPI.Services.Travelport;
using MongoDB.Driver;
using OnionConsumeWebAPI.Models;
using OnionConsumeWebAPI.Comman;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using static DomainLayer.Model.SeatMapResponceModel;

namespace OnionConsumeWebAPI.Controllers.AirAsia
{
    //[Route("jetways/[controller]/[action]")]
    public class FlightSearchIndexController : Controller
    {

        public IActionResult MyBooking()
        {

            return View();
        }
        // Mongo DB
        // private readonly MongoDbService _mongoDbService;

        public readonly IDistributedCache _distributedCache;
        private readonly CredentialService _credentialService;
        private readonly IConfiguration _configuration;
        public FlightSearchIndexController(IDistributedCache distributedcache, CredentialService credentialService, IConfiguration configuration)
        {
            _distributedCache = distributedcache;
            //_mongoDbService = mongoDbService;
            _credentialService = credentialService;
            _configuration = configuration;

        }
        private string KeyName = string.Empty;
        public static int counterRedis = 0;
        public static int counterapihit = 0;
        //    string BaseURL = "https://dotrezapi.test.I5.navitaire.com";

        string token = string.Empty;
        int adultcount = 0;
        int childcount = 0;
        int _infantcount = 0;
        int TotalCount = 0;




        [Route("")]
        public async Task<IActionResult> Index()
        {
            //List<City> city = new List<City>();
            //HttpClient client = new HttpClient();
            ////client.BaseAddress = new Uri("http://localhost:5225/");
            //client.BaseAddress = new Uri(AppUrlConstant.BaseURL);
            //HttpResponseMessage response = await client.GetAsync("api/City/getallCity");
            //if (response.IsSuccessStatusCode)
            //{
            //    var results = response.Content.ReadAsStringAsync().Result;
            //    city = JsonConvert.DeserializeObject<List<City>>(results);
            //    city.Insert(0, new City { CityCode = "Select", CityName = "Select" });
            //}
            //else
            //{
            //    city.Insert(0, new City { CityCode = "Select", CityName = "Select" });
            //}
            //ViewBag.ListofCountry = city;

            return View();
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SearchResultFlight(SimpleAvailabilityRequestModel _GetfligthModel, string flightclass, string SameAirlineRT)
        {
            TempData["RoundTripRadioButton"] = SameAirlineRT;
            TempData["RoundTripSameAirline"] = SameAirlineRT;
            if (SameAirlineRT.ToLower() == "airlinert")
            {
                TempData["FlightModel"] = JsonConvert.SerializeObject(_GetfligthModel);
                return RedirectToAction("FlightSameAirline", "FlightSearchIndexRT", new { flightclass = flightclass, SameAirlineRT = SameAirlineRT });
            }
            else
            {
                //HttpContext.Session.Remove("SameAirlineRT");
            }

            if (_GetfligthModel.passengercount != null)
            {
                KeyName = _GetfligthModel.origin + "_" + _GetfligthModel.destination + "_" + _GetfligthModel.beginDate + "_" + _GetfligthModel.passengercount.adultcount;
            }
            else
            {
                KeyName = _GetfligthModel.origin + "_" + _GetfligthModel.destination + "_" + _GetfligthModel.beginDate + "_" + _GetfligthModel.adultcount;
            }
            // Mongo DB


            //START:21-12-2024 SAVE SEARCH LOG DATA
            string SearchGuid = Guid.NewGuid().ToString().ToUpper();
            string ResponseGuid = string.Empty;
            string getguid = string.Empty;

            MongoHelper objMongoHelper = new MongoHelper();
            MongoDBHelper _mongoDBHelper = new MongoDBHelper(_configuration);
            List<SimpleAvailibilityaAddResponce> addResponces = new List<SimpleAvailibilityaAddResponce>();

            //getguid = _mongoDBHelper.GetFlightSearchByKeyRef(objMongoHelper.GetRequestCacheKey(_GetfligthModel)).Result;

            //_mongoDBHelper.SaveSearchLog(_GetfligthModel, SearchGuid);

            //if (string.IsNullOrEmpty(getguid))
            //{
            //    _mongoDBHelper.SaveKeyRequest(SearchGuid, objMongoHelper.GetRequestCacheKey(_GetfligthModel));
            //    _mongoDBHelper.SaveRequest(_GetfligthModel, SearchGuid);
            //    ResponseGuid = SearchGuid;
            //}

            //END
            List<SimpleAvailibilityaAddResponce> SimpleAvailibilityaAddResponcelist = new List<SimpleAvailibilityaAddResponce>();
            if (_GetfligthModel == null)
            {
                return RedirectToAction("Index", "FlightSearchIndex");
            }
            //caching
            Logs logs = new Logs();
            IHttpContextAccessor httpContextAccessorInstance = new HttpContextAccessor();
            string searlizetext = string.Empty;
            string _simpleAvailability = string.Empty;
            //string File = @"D:\Data\HitLogs.txt"; // Path to your text file
            //var encodedlist = await _distributedCache.GetAsync(KeyName);
            string encodedlist = await _distributedCache.GetStringAsync(KeyName);
            if (encodedlist != null)
            {
                counterRedis++;
                SimpleAvailibilityaAddResponcelist = new List<SimpleAvailibilityaAddResponce>();
                //searlizetext = Encoding.UTF8.GetString(encodedlist);
                //SimpleAvailibilityaAddResponcelist = JsonConvert.DeserializeObject<List<SimpleAvailibilityaAddResponce>>(searlizetext);
                SimpleAvailibilityaAddResponcelist = JsonConvert.DeserializeObject<List<SimpleAvailibilityaAddResponce>>(encodedlist);
                // Write initial content to the file
                logs.WriteToFile(KeyName + "_RedisCounter=" + counterRedis);
                return RedirectToAction("FlightView", "ResultFlightView");
            }
            else
            {
                counterapihit++;
                logs.WriteToFile(KeyName + "_ApiHitCounter=" + counterapihit);
                string destination1 = string.Empty;
                string origin = string.Empty;
                string arrival1 = string.Empty;
                string departure1 = string.Empty;
                string identifier1 = string.Empty;
                string carrierCode1 = string.Empty;
                string totalfare1 = string.Empty;
                string journeyKey1 = string.Empty;
                string fareAvailabilityKey1 = string.Empty;
                string inventoryControl1 = string.Empty;
                string ssrKey = string.Empty;
                string passengerkey = string.Empty;
                string uniquekey = string.Empty;
                decimal fareTotalsum = 0;
                string formatTime = string.Empty;

                if (_GetfligthModel.passengercount != null)
                {
                    adultcount = _GetfligthModel.passengercount.adultcount;
                    childcount = _GetfligthModel.passengercount.childcount;
                    _infantcount = _GetfligthModel.passengercount.infantcount;
                    TotalCount = adultcount + childcount + _infantcount;
                }
                else
                {
                    adultcount = _GetfligthModel.adultcount;
                    childcount = _GetfligthModel.childcount;
                    _infantcount = _GetfligthModel.infantcount;
                    TotalCount = adultcount + childcount + _infantcount;
                }

                HttpContext.Session.SetString("adultCount", JsonConvert.SerializeObject(adultcount));
                HttpContext.Session.SetString("childCount", JsonConvert.SerializeObject(childcount));
                HttpContext.Session.SetString("infantCount", JsonConvert.SerializeObject(_infantcount));


                // _credentials credentialsobj = new _credentials();
                using (HttpClient client = new HttpClient())
                {
                    //client.BaseAddress = new Uri("http://localhost:5225/");
                    client.BaseAddress = new Uri(AppUrlConstant.BaseURL);
                    HttpResponseMessage response = await client.GetAsync("api/Login/getotacredairasia");
                    //Air Asia login
                    var _credentialsAirasia = new _credentials();
                    if (response.IsSuccessStatusCode)
                    {
                        await _credentialService.PopulateCredentialsAsync(response, _credentialsAirasia, 1);

                    }

                    airlineLogin login = new airlineLogin();
                    login.credentials = _credentialsAirasia;

                    TempData["AirAsiaLogin"] = login.credentials.Image;
                    AirasiaTokan AirasiaTokan = new AirasiaTokan();
                    var AirasialoginRequest = JsonConvert.SerializeObject(login, Formatting.Indented);
                    logs.WriteLogs(AirasialoginRequest, "1-Tokan_Request", "AirAsiaOneWay", SameAirlineRT);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    //client.Timeout = TimeSpan.FromSeconds(10);
                    HttpResponseMessage responce = await client.PostAsJsonAsync(AppUrlConstant.AirasiaTokan, login);

                    if (responce.IsSuccessStatusCode)
                    {
                        var results = responce.Content.ReadAsStringAsync().Result;
                        logs.WriteLogs(results, "1-Token_Responce", "AirAsiaOneWay", SameAirlineRT);
                        var JsonObj = JsonConvert.DeserializeObject<dynamic>(results);
                        AirasiaTokan.token = JsonObj.data.token;
                        AirasiaTokan.idleTimeoutInMinutes = JsonObj.data.idleTimeoutInMinutes;
                        //token = ((Newtonsoft.Json.Linq.JValue)value).Value.ToString();
                    }



                    //MongoHelper objMongoHelper = new MongoHelper();
                    //MongoDBHelper _mongoDBHelper = new MongoDBHelper(_mongoDbService);

                    //getguid = _mongoDBHelper.GetFlightSearchByKeyRef(objMongoHelper.GetRequestCacheKey(_GetfligthModel)).Result;

                    //_mongoDBHelper.SaveSearchLog(_GetfligthModel, SearchGuid);

                    //if (string.IsNullOrEmpty(getguid))
                    //{
                    //    _mongoDBHelper.SaveRequest(_GetfligthModel, SearchGuid);
                    //}

                    //var searchData = new SearchLog
                    //{
                    //    TripType = 0,
                    //    TripName = "OneWay",
                    //    ApiName= "Logon",
                    //    SupplierName= "AirIndiaExpress",
                    //    Origin_Departure = _GetfligthModel.origin.Split("-")[1] + "_" + _GetfligthModel.destination.Split("-")[1],
                    //    Key = KeyName,
                    //    Request = AirasialoginRequest,
                    //    Response = JsonConvert.SerializeObject(AirasiaTokan.token),
                    //    InsertedOn = DateTime.Now
                    //};
                    //coll.InsertOne(searchData );
                    //logs.WriteLogs("Request: " + AirasialoginRequest + "\n Response: " + JsonConvert.SerializeObject(AirasiaTokan.token), "Logon", "AirAsiaOneWay");


                    HttpContext.Session.SetString("AirasiaTokan", JsonConvert.SerializeObject(AirasiaTokan.token));



                    SimpleAvailabilityRequestModel _SimpleAvailabilityobj = new SimpleAvailabilityRequestModel();
                    string orgincity = string.Empty;
                    string destinationcode = string.Empty;
                    string destinationCity = string.Empty;
                    string orgincode = string.Empty;

                    string input = _GetfligthModel.origin;
                    string[] parts = input.Split('-');

                    if (parts.Length == 2)
                    {
                        orgincity = parts[0].Trim(); // Contains "New Delhi"
                        orgincode = parts[1].Trim(); // Contains "DEL"
                        _GetfligthModel.origin = orgincode;
                    }
                    input = _GetfligthModel.destination;
                    parts = input.Split('-');
                    if (parts.Length == 2)
                    {
                        destinationCity = parts[0].Trim(); // Contains "New Delhi"
                        destinationcode = parts[1].Trim(); // Contains "DEL"
                        _GetfligthModel.destination = destinationcode;
                    }
                    _SimpleAvailabilityobj.origin = _GetfligthModel.origin;
                    _SimpleAvailabilityobj.destination = _GetfligthModel.destination;
                    _SimpleAvailabilityobj.beginDate = _GetfligthModel.beginDate;
                    //if (_GetfligthModel.endDate == null)
                    //{
                    //    _SimpleAvailabilityobj.endDate = _GetfligthModel.beginDate;
                    //}
                    //else
                    //_SimpleAvailabilityobj.endDate = _GetfligthModel.endDate;


                    var AdtType = string.Empty;
                    var AdtCount = 0;
                    var chdtype = string.Empty;
                    var chdcount = 0;
                    var infanttype = string.Empty;
                    var infantcount = 0;
                    Codessimple _codes = new Codessimple();
                    List<Typesimple> _typeslist = new List<Typesimple>();
                    if (_GetfligthModel.passengercount != null)
                    {
                        AdtType = _GetfligthModel.passengercount.adulttype;
                        AdtCount = _GetfligthModel.passengercount.adultcount;
                        chdtype = _GetfligthModel.passengercount.childtype;
                        chdcount = _GetfligthModel.passengercount.childcount;
                        infanttype = _GetfligthModel.passengercount.infanttype;
                        infantcount = _GetfligthModel.passengercount.infantcount;
                    }
                    else
                    {
                        AdtType = _GetfligthModel.adulttype;
                        AdtCount = _GetfligthModel.adultcount;
                        chdtype = _GetfligthModel.childtype;
                        chdcount = _GetfligthModel.childcount;
                        infanttype = _GetfligthModel.infanttype;
                        infantcount = _GetfligthModel.infantcount;
                    }
                    if (AdtType == "ADT" && AdtCount != 0)
                    {
                        Typesimple Types = new Typesimple();
                        Types.type = AdtType;
                        Types.count = AdtCount;
                        _typeslist.Add(Types);
                    }
                    if (chdtype == "CHD" && chdcount != 0)
                    {
                        Typesimple Types = new Typesimple();
                        Types.type = chdtype;
                        Types.count = chdcount;
                        _typeslist.Add(Types);
                    }
                    if (infanttype == "INFT" && infantcount != 0)
                    {
                        Typesimple Types = new Typesimple();
                        Types.type = infanttype;
                        Types.count = infantcount;
                        _typeslist.Add(Types);
                    }
                    Passengerssimple _Passengerssimple = new Passengerssimple();
                    _Passengerssimple.types = _typeslist;
                    _SimpleAvailabilityobj.passengers = _Passengerssimple;
                    //_codes.currencyCode = "INR";
                    _SimpleAvailabilityobj.codes = _codes;
                    _SimpleAvailabilityobj.sourceOrganization = "";
                    _SimpleAvailabilityobj.currentSourceOrganization = "";
                    _SimpleAvailabilityobj.promotionCode = "OTAPROMO";
                    string[] sortOptions = new string[1];
                    sortOptions[0] = "ServiceType";
                    // Define the Filters class
                    Filters Filters = new Filters();

                    // Define fare types and product classes based on flight class
                    if (flightclass == "B")
                    {
                        Filters.fareTypes = new[] { "R" };
                        Filters.productClasses = new[] { "VV" };
                    }
                    else
                    {
                        Filters.fareTypes = new[] { "R", "M" };
                        Filters.productClasses = new[] { "EC", "EP", "HF" };
                    }
                    Filters.exclusionType = "Default";
                    Filters.loyalty = "MonetaryOnly";
                    Filters.includeAllotments = true;
                    Filters.connectionType = "Both";
                    Filters.compressionType = "CompressByProductClass";
                    Filters.sortOptions = sortOptions;
                    Filters.maxConnections = 10;

                    _SimpleAvailabilityobj.filters = Filters;
                    _SimpleAvailabilityobj.taxesAndFees = "Taxes";
                    _SimpleAvailabilityobj.ssrCollectionsMode = "Leg";
                    _SimpleAvailabilityobj.numberOfFaresPerJourney = 10;
                    //List<SimpleAvailibilityaAddResponce> SimpleAvailibilityaAddResponcelist = new List<SimpleAvailibilityaAddResponce>();
                    SimpleAvailibilityaAddResponce _SimpleAvailibilityaAddResponceobj = new SimpleAvailibilityaAddResponce();
                    List<SimpleAvailibilityaAddResponce> SimpleAvailibilityaAddResponcelistR = new List<SimpleAvailibilityaAddResponce>();
                    SimpleAvailibilityaAddResponce _SimpleAvailibilityaAddResponceobjR = new SimpleAvailibilityaAddResponce();
                    var json = JsonConvert.SerializeObject(_SimpleAvailabilityobj, Formatting.Indented);
                    logs.WriteLogs(json, "2-SimpleAvailability_Req", "AirAsiaOneWay", SameAirlineRT);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AirasiaTokan.token);
                    HttpResponseMessage responce1 = await client.PostAsJsonAsync(AppUrlConstant.Airasiasearchsimple, _SimpleAvailabilityobj);
                    int uniqueidx = 0;
                    if (responce1.IsSuccessStatusCode)
                    {
                        var results = responce1.Content.ReadAsStringAsync().Result;
                        logs.WriteLogs(results, "2-SimpleAvailability_Res", "AirAsiaOneWay", SameAirlineRT);
                        
                        var JsonObj = JsonConvert.DeserializeObject<dynamic>(results);
                        dynamic jsonObj = JObject.Parse(results);

                        var oriDes = _GetfligthModel.origin + "|" + _GetfligthModel.destination;
                        TempData["origin"] = _SimpleAvailabilityobj.origin;
                        TempData["destination"] = _SimpleAvailabilityobj.destination;
                        
                        if (jsonObj.data.results != null && ((JArray)jsonObj.data.results).Count > 0)
                        {
                            var finddate = JsonObj.data.results[0].trips[0].date;
                            var bookingdate = finddate.ToString("dddd, dd MMMM yyyy");
                            int count = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes].Count;
                            TempData["count"] = count;
                                                      

                            for (int i = 0; i < JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes].Count; i++)
                            {
                                var journeyData = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i];
                                int stops = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].stops;
                                string journeyKey = journeyData.journeyKey;
                                var uniqueJourney = journeyData;
                                Designator Designatorobj = new Designator();
                                string queryorigin = journeyData.designator.origin;
                                origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                                Designatorobj.origin = origin;
                                string querydestination = journeyData.designator.destination;
                                destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                Designatorobj.destination = destination1;

                                Designatorobj.departure = journeyData.designator.departure;
                                Designatorobj.arrival = journeyData.designator.arrival;
                                Designatorobj.Arrival = journeyData.designator.arrival;
                                DateTime arrivalDateTime = DateTime.ParseExact(Designatorobj.Arrival, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                                Designatorobj.ArrivalDate = arrivalDateTime.ToString("yyyy-MM-dd");
                                Designatorobj.ArrivalTime = arrivalDateTime.ToString("HH:mm:ss");
                                TimeSpan travelTimeDiff = Designatorobj.arrival - Designatorobj.departure;
                                TimeSpan timeSpan = TimeSpan.Parse(travelTimeDiff.ToString());
                                if ((int)timeSpan.Minutes == 0)
                                    formatTime = $"{(int)timeSpan.TotalHours} h";
                                else
                                    formatTime = $"{(int)timeSpan.TotalHours} h {(int)timeSpan.Minutes} m";
                                Designatorobj.formatTime = timeSpan;
                                //Vivek sir
                                //Designatorobj.SetformatTime = formatTime;
                                var segmentscount = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments.Count;
                                List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();

                                for (int l = 0; l < segmentscount; l++)
                                {
                                    DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                    Designator SegmentDesignatorobj = new Designator();
                                    var journeysegments = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l];

                                    SegmentDesignatorobj.origin = journeysegments.designator.origin;
                                    SegmentDesignatorobj.destination = journeysegments.designator.destination;
                                    SegmentDesignatorobj.departure = journeysegments.designator.departure;
                                    SegmentDesignatorobj.arrival = journeysegments.designator.arrival;
                                    Segmentobj.designator = SegmentDesignatorobj;
                                    Identifier Identifier = new Identifier();
                                    Identifier.identifier = journeysegments.identifier.identifier;
                                    Identifier.carrierCode = journeysegments.identifier.carrierCode;
                                    Segmentobj.identifier = Identifier;

                                    int legscount = journeysegments.legs.Count;
                                    List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();

                                    for (int m = 0; m < legscount; m++)
                                    {
                                        DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                        Designator legdesignatorobj = new Designator();
                                        var jursegleg = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l].legs[m];
                                        queryorigin = jursegleg.designator.origin;
                                        querydestination = jursegleg.designator.destination;
                                      
                                        if (Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault() != null)
                                        {
                                            origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().citycode;
                                            legdesignatorobj.origin = origin;
                                        }
                                        else
                                        {
                                            legdesignatorobj.origin = journeysegments.designator.origin;
                                        }
                                        if (Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault() != null)
                                        {
                                            destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().citycode;
                                            legdesignatorobj.destination = destination1;
                                        }
                                        else
                                        {
                                            legdesignatorobj.destination = journeysegments.designator.destination;
                                        }

                                        legdesignatorobj.departure = jursegleg.designator.departure;
                                        legdesignatorobj.arrival = jursegleg.designator.arrival;
                                        Legobj.designator = legdesignatorobj;
                                        Legobj.legKey = jursegleg.legKey;
                                        Legobj.flightReference = jursegleg.flightReference;
                                        Leglist.Add(Legobj);
                                        DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                        LegInfo.arrivalTerminal = jursegleg.legInfo.arrivalTerminal;
                                        LegInfo.departureTerminal = jursegleg.legInfo.departureTerminal;
                                        LegInfo.arrivalTime = jursegleg.legInfo.arrivalTime;
                                        LegInfo.departureTime = jursegleg.legInfo.departureTime;
                                        Legobj.legInfo = LegInfo;

                                    }
                                    Segmentobj.legs = Leglist;
                                    Segmentobjlist.Add(Segmentobj);

                                }
                                var Terminal = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[0].legs[0];
                                var arrivalTerminal = Terminal.legInfo.arrivalTerminal;
                                var departureTerminal = Terminal.legInfo.departureTerminal;
                                int FareCount = journeyData.fares.Count;
                                


                                if (FareCount > 0)
                                {
                                    List<FareIndividual> fareIndividualsList = new List<FareIndividual>();

                                    for (int j = 0; j < FareCount; j++)
                                    {
                                        //x.data.results[0].trips[0].journeysAvailableByMarket["DEL|BLR"][0].fares[0].fareAvailabilityKey


                                        FareIndividual fareIndividual = new FareIndividual();

                                        var fareAvailability = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i];
                                        string fareAvailabilityKey = fareAvailability.fares[j].fareAvailabilityKey;
                                        //fareIndividual.faretotal = JsonObj.data.faresAvailable[fareAvailabilityKey].faretotal;
                                        Total total = new Total();
                                        var booking = JsonObj.data.faresAvailable[fareAvailabilityKey];
                                        var bookingamount = booking.totals.fareTotal;

                                        string fareAvailabilityKeyhead = fareAvailability.fares[0].fareAvailabilityKey;
                                        var fareAvilableCount = booking.fares.Count;
                                        var isGoverning = booking.fares[0].isGoverning;

                                        var procuctclass = booking.fares[0].productClass;

                                        var passengertype = booking.fares[0].passengerFares[0].passengerType;
                                        //Start :comment booking amount not correct
                                        // var fareAmount = JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].fareAmount;
                                        // fareTotalsum = JsonObj.data.faresAvailable[fareAvailabilityKeyhead].fares[0].passengerFares[0].fareAmount;
                                        //End:
                                        //ADD New Start
                                        int passengercount = adultcount + chdcount;
                                        var perpersontotal = booking.totals.fareTotal;
                                        var fareAmount = perpersontotal / passengercount;
                                        var perpersontotalclasswise = booking.totals.fareTotal;
                                        if (j == 0)
                                        {
                                            fareTotalsum = perpersontotalclasswise / passengercount;
                                        }

                                        //END
                                        decimal discountamount = booking.fares[0].passengerFares[0].discountedFare;

                                        int servicecharge = booking.fares[0].passengerFares[0].serviceCharges.Count;
                                        decimal finalamount = 0;
                                        for (int k = 1; k < servicecharge; k++)
                                        {

                                            decimal amount = booking.fares[0].passengerFares[0].serviceCharges[k].amount;
                                            finalamount += amount;

                                        }
                                        //TempData["fareTotalsum"] = fareTotalsum;

                                        decimal taxamount = finalamount;
                                        fareIndividual.taxamount = taxamount;
                                        fareIndividual.faretotal = fareAmount;
                                        fareIndividual.discountamount = discountamount;
                                        fareIndividual.passengertype = passengertype;
                                        fareIndividual.fareKey = fareAvailabilityKey;
                                        fareIndividual.procuctclass = procuctclass;
                                        fareIndividualsList.Add(fareIndividual);

                                    }

                                    var expandoconverter = new ExpandoObjectConverter();
                                    dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(uniqueJourney.ToString(), expandoconverter);
                                    string jsonresult = JsonConvert.SerializeObject(obj);
                                    //to do
                                    _SimpleAvailibilityaAddResponceobj = JsonConvert.DeserializeObject<SimpleAvailibilityaAddResponce>(jsonresult);

                                    _SimpleAvailibilityaAddResponceobj.designator = Designatorobj;
                                    _SimpleAvailibilityaAddResponceobj.segments = Segmentobjlist;
                                    _SimpleAvailibilityaAddResponceobj.arrivalTerminal = arrivalTerminal;
                                    _SimpleAvailibilityaAddResponceobj.departureTerminal = departureTerminal;
                                    _SimpleAvailibilityaAddResponceobj.bookingdate = bookingdate;
                                    _SimpleAvailibilityaAddResponceobj.fareTotalsum = fareTotalsum;
                                    _SimpleAvailibilityaAddResponceobj.journeyKey = journeyKey;
                                    _SimpleAvailibilityaAddResponceobj.faresIndividual = fareIndividualsList;
                                    //_SimpleAvailibilityaAddResponceobj.uniqueId = i;
                                    _SimpleAvailibilityaAddResponceobj.Airline = Airlines.Airasia;
                                    _SimpleAvailibilityaAddResponceobj.uniqueId = uniqueidx;
                                    if (_SimpleAvailibilityaAddResponceobj.fareTotalsum <= 0 || stops ==2)
                                        continue;
                                    uniqueidx++;
                                    SimpleAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                                }
                            }
                        }

                    }
                    GetAvailabilityRequest _getAvailabilityRQ = null;
                    if (flightclass != "B")
                    {
                        #region Akasha
                        //Akasa login
                        _credentials _CredentialsAkasha = new _credentials();
                        if (response.IsSuccessStatusCode)
                        {

                            await _credentialService.PopulateCredentialsAsync(response, _CredentialsAkasha, 2);


                        }

                        airlineLogin loginobject = new airlineLogin();
                        loginobject.credentials = _CredentialsAkasha;
                        //TempData["AirAsiaLogin"] = login.credentials.Image;
                        AirasiaTokan AkasaTokan = new AirasiaTokan();
                        var AkasaloginRequest  = JsonConvert.SerializeObject(loginobject, Formatting.Indented);
                        logs.WriteLogs(AkasaloginRequest, "1-Tokan_Request", "AkasaOneWay", SameAirlineRT);
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage responcedata = await client.PostAsJsonAsync(AppUrlConstant.AkasaTokan, loginobject);
                        if (responcedata.IsSuccessStatusCode)
                        {
                            var results = responcedata.Content.ReadAsStringAsync().Result;
                            logs.WriteLogs(results, "1-Token_Responce", "AkasaOneWay", SameAirlineRT);
                            var JsonObj = JsonConvert.DeserializeObject<dynamic>(results);
                            AkasaTokan.token = JsonObj.data.token;
                            AkasaTokan.idleTimeoutInMinutes = JsonObj.data.idleTimeoutInMinutes;
                            HttpContext.Session.SetString("AkasaTokan", JsonConvert.SerializeObject(AkasaTokan.token));
                            _SimpleAvailabilityobj = new DomainLayer.Model.SimpleAvailabilityRequestModel();
                            _SimpleAvailabilityobj.origin = _GetfligthModel.origin;
                            _SimpleAvailabilityobj.destination = _GetfligthModel.destination;
                            _SimpleAvailabilityobj.searchDestinationMacs = true;
                            _SimpleAvailabilityobj.searchOriginMacs = true;
                            _SimpleAvailabilityobj.beginDate = _GetfligthModel.beginDate;
                            //_SimpleAvailabilityobj.endDate = _GetfligthModel.endDate; //"2023-12-20";//_GetfligthModel.endDate;
                            _SimpleAvailabilityobj.getAllDetails = true;
                            _SimpleAvailabilityobj.taxesAndFees = "TaxesAndFees";
                            _codes = new Codessimple();
                            _typeslist = new List<Typesimple>();
                            if (_GetfligthModel.passengercount != null)
                            {
                                AdtType = _GetfligthModel.passengercount.adulttype;
                                AdtCount = _GetfligthModel.passengercount.adultcount;
                                chdtype = _GetfligthModel.passengercount.childtype;
                                chdcount = _GetfligthModel.passengercount.childcount;
                                infanttype = _GetfligthModel.passengercount.infanttype;
                                infantcount = _GetfligthModel.passengercount.infantcount;
                            }
                            else
                            {
                                AdtType = _GetfligthModel.adulttype;
                                AdtCount = _GetfligthModel.adultcount;
                                chdtype = _GetfligthModel.childtype;
                                chdcount = _GetfligthModel.childcount;
                                infanttype = _GetfligthModel.infanttype;
                                infantcount = _GetfligthModel.infantcount;
                            }
                            if (AdtType == "ADT" && AdtCount != 0)
                            {
                                Typesimple Types = new Typesimple();
                                Types.type = AdtType;
                                Types.count = AdtCount;
                                _typeslist.Add(Types);
                            }
                            if (chdtype == "CHD" && chdcount != 0)
                            {
                                Typesimple Types = new Typesimple();
                                Types.type = chdtype;
                                Types.count = chdcount;
                                _typeslist.Add(Types);
                            }
                            if (infanttype == "INFT" && infantcount != 0)
                            {
                                Typesimple TypesR = new Typesimple();
                                TypesR.type = infanttype;
                                TypesR.count = infantcount;
                                _typeslist.Add(TypesR);
                            }
                            _Passengerssimple = new Passengerssimple();
                            _Passengerssimple.types = _typeslist;
                            _SimpleAvailabilityobj.passengers = _Passengerssimple;
                            _SimpleAvailabilityobj.codes = _codes;
                            sortOptions = new string[1];
                            sortOptions[0] = "NoSort";
                            string[] fareTypes = new string[3];
                            fareTypes[0] = "R";
                            fareTypes[1] = "V";
                            fareTypes[2] = "S";
                            string[] productClasses = new string[3];
                            productClasses[0] = "EC";
                            productClasses[1] = "AV";
                            productClasses[2] = "SP";
                            Filters = new Filters();
                            Filters.compressionType = "1";
                            Filters.groupByDate = false;
                            Filters.carrierCode = "QP";
                            Filters.type = "ALL";
                            Filters.sortOptions = sortOptions;
                            Filters.maxConnections = 4;
                            Filters.fareTypes = fareTypes;
                            Filters.productClasses = productClasses;
                            _SimpleAvailabilityobj.filters = Filters;
                            _SimpleAvailabilityobj.numberOfFaresPerJourney = 4;
                            json = JsonConvert.SerializeObject(_SimpleAvailabilityobj, Formatting.Indented);
                            logs.WriteLogs(json, "2-SimpleAvailability_Req", "AkasaOneWay", SameAirlineRT);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AkasaTokan.token);
                            HttpResponseMessage responceAkasaAir = await client.PostAsJsonAsync(AppUrlConstant.AkasaAirSearchSimple, _SimpleAvailabilityobj);
                            //uniqueidx = 0;
                            if (responceAkasaAir.IsSuccessStatusCode)
                            {

                                var resultsAkasaAir = responceAkasaAir.Content.ReadAsStringAsync().Result;
                                logs.WriteLogs(resultsAkasaAir, "2-SimpleAvailability_Res", "AkasaOneWay", SameAirlineRT);
                                //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_SimpleAvailabilityobj) + "\n Response: " + resultsAkasaAir, "GetAvailability", "AkasaOneWay");
                                var JsonAkasaAir = JsonConvert.DeserializeObject<dynamic>(resultsAkasaAir);
                                dynamic jsonAkasaAir = JObject.Parse(resultsAkasaAir);

                                var oriDes = _GetfligthModel.origin + "|" + _GetfligthModel.destination;
                                TempData["origin"] = _SimpleAvailabilityobj.origin;
                                TempData["destination"] = _SimpleAvailabilityobj.destination;
                                if (jsonAkasaAir.data.results != null && ((JArray)jsonAkasaAir.data.results).Count > 0)
                                {
                                    if (((JArray)jsonAkasaAir.data.results[0].trips).Count > 0)
                                    {
                                        //var finddate = JsonObj.data.results[0].trips[0].date;
                                        var finddate = JsonAkasaAir.data.results[0].trips[0].date;
                                        var bookingdate = finddate.ToString("dddd, dd MMMM yyyy");
                                        int count = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes].Count;
                                        TempData["count"] = count;
                                        for (int i = 0; i < JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes].Count; i++)
                                        {
                                            var journey = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i];
                                            var stops = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].stops;
                                            string journeyKey = journey.journeyKey;
                                            var uniqueJourney = journey;

                                            Designator AkasaDesignatorobj = new Designator();
                                            string queryorigin = journey.designator.origin;
                                            origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                                            AkasaDesignatorobj.origin = origin;
                                            string querydestination = journey.designator.destination;
                                            destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                            AkasaDesignatorobj.destination = destination1;

                                            AkasaDesignatorobj.departure = journey.designator.departure;
                                            AkasaDesignatorobj.arrival = journey.designator.arrival;
                                            AkasaDesignatorobj.Arrival = journey.designator.arrival;
                                            DateTime AarrivalDateTime = DateTime.ParseExact(AkasaDesignatorobj.Arrival, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                            //Arrival = Designatorobj.Arrival,
                                            AkasaDesignatorobj.ArrivalDate = AarrivalDateTime.ToString("yyyy-MM-dd");
                                            AkasaDesignatorobj.ArrivalTime = AarrivalDateTime.ToString("HH:mm:ss");
                                            TimeSpan travelTimeDiff = AkasaDesignatorobj.arrival - AkasaDesignatorobj.departure;
                                            TimeSpan timeSpan = TimeSpan.Parse(travelTimeDiff.ToString());
                                            if ((int)timeSpan.Minutes == 0)
                                                formatTime = $"{(int)timeSpan.TotalHours} h";
                                            else
                                                formatTime = $"{(int)timeSpan.TotalHours} h {(int)timeSpan.Minutes} m";
                                            AkasaDesignatorobj.formatTime = timeSpan;
                                            //vivek
                                            //AkasaDesignatorobj.SetformatTime = formatTime;
                                            var segmentscount = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments.Count;
                                            List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();

                                            for (int l = 0; l < segmentscount; l++)
                                            {
                                                DomainLayer.Model.Segment AkasaSegmentobj = new DomainLayer.Model.Segment();
                                                Designator AkasaSegmentDesignatorobj = new Designator();
                                                //queryorigin = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l].designator.origin;
                                                // querydestination = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l].designator.destination;                       
                                                var jouseg = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l];
                                                AkasaSegmentDesignatorobj.origin = jouseg.designator.origin;
                                                AkasaSegmentDesignatorobj.destination = jouseg.designator.destination;
                                                AkasaSegmentDesignatorobj.departure = jouseg.designator.departure;
                                                AkasaSegmentDesignatorobj.arrival = jouseg.designator.arrival;
                                                AkasaSegmentobj.designator = AkasaSegmentDesignatorobj;
                                                Identifier AkasaIdentifier = new Identifier();
                                                AkasaIdentifier.identifier = jouseg.identifier.identifier;
                                                AkasaIdentifier.carrierCode = jouseg.identifier.carrierCode;
                                                AkasaSegmentobj.identifier = AkasaIdentifier;

                                                int Akasalegscount = jouseg.legs.Count;
                                                List<DomainLayer.Model.Leg> AkasaLeglist = new List<DomainLayer.Model.Leg>();

                                                for (int m = 0; m < Akasalegscount; m++)
                                                {
                                                    DomainLayer.Model.Leg AkasaLegobj = new DomainLayer.Model.Leg();
                                                    Designator Akasalegdesignatorobj = new Designator();
                                                    var jousegleg = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l].legs[m];
                                                    queryorigin = jousegleg.designator.origin;
                                                    querydestination = jousegleg.designator.destination;
                                                    if (Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault() != null)
                                                    {
                                                        origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().citycode;
                                                        Akasalegdesignatorobj.origin = origin;
                                                    }
                                                    else
                                                    {
                                                        Akasalegdesignatorobj.origin = jouseg.designator.origin;
                                                    }
                                                    if (Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault() != null)
                                                    {
                                                        destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().citycode;
                                                        Akasalegdesignatorobj.destination = destination1;
                                                    }
                                                    else
                                                    {
                                                        Akasalegdesignatorobj.destination = jouseg.designator.destination;
                                                    }

                                                    Akasalegdesignatorobj.departure = jousegleg.designator.departure;
                                                    Akasalegdesignatorobj.arrival = jousegleg.designator.arrival;
                                                    AkasaLegobj.designator = Akasalegdesignatorobj;
                                                    AkasaLegobj.legKey = jousegleg.legKey;
                                                    AkasaLegobj.flightReference = jousegleg.flightReference;
                                                    AkasaLeglist.Add(AkasaLegobj);
                                                    DomainLayer.Model.LegInfo AkasaLegInfo = new DomainLayer.Model.LegInfo();
                                                    AkasaLegInfo.arrivalTerminal = jousegleg.legInfo.arrivalTerminal;
                                                    AkasaLegInfo.departureTerminal = jousegleg.legInfo.departureTerminal;
                                                    AkasaLegInfo.arrivalTime = jousegleg.legInfo.arrivalTime;
                                                    AkasaLegInfo.departureTime = jousegleg.legInfo.departureTime;
                                                    AkasaLegobj.legInfo = AkasaLegInfo;

                                                }
                                                AkasaSegmentobj.legs = AkasaLeglist;
                                                Segmentobjlist.Add(AkasaSegmentobj);

                                            }
                                            var Terminal = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[0].legs[0].legInfo;
                                            var arrivalTerminal = Terminal.arrivalTerminal;
                                            var departureTerminal = Terminal.departureTerminal;
                                            int AkasaFareCount = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].fares.Count;
                                            _SimpleAvailibilityaAddResponceobj = new SimpleAvailibilityaAddResponce();
                                            if (AkasaFareCount > 0)
                                            {
                                                List<FareIndividual> AkasafareIndividualsList = new List<FareIndividual>();

                                                for (int j = 0; j < AkasaFareCount; j++)
                                                {
                                                    //x.data.results[0].trips[0].journeysAvailableByMarket["DEL|BLR"][0].fares[0].fareAvailabilityKey


                                                    FareIndividual AkasafareIndividual = new FareIndividual();


                                                    string fareAvailabilityKey = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].fares[j].fareAvailabilityKey;
                                                    //fareIndividual.faretotal = JsonObj.data.faresAvailable[fareAvailabilityKey].faretotal;
                                                    Total Akasatotal = new Total();
                                                    var bookingamount = JsonAkasaAir.data.faresAvailable[fareAvailabilityKey].totals.fareTotal;

                                                    string fareAvailabilityKeyhead = JsonAkasaAir.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].fares[0].fareAvailabilityKey;
                                                    var fareAvilableCount = JsonAkasaAir.data.faresAvailable[fareAvailabilityKey].fares.Count;
                                                    var fare = JsonAkasaAir.data.faresAvailable[fareAvailabilityKey];
                                                    var isGoverning = fare.fares[0].isGoverning;

                                                    var procuctclass = fare.fares[0].productClass;

                                                    var passengertype = fare.fares[0].passengerFares[0].passengerType;

                                                    int passengercount = adultcount + chdcount;
                                                    var perpersontotal = fare.totals.fareTotal;
                                                    var fareAmount = perpersontotal / passengercount;
                                                    var perpersontotalclasswise = fare.totals.fareTotal;
                                                    if (j == 0)
                                                    {
                                                        fareTotalsum = perpersontotalclasswise / passengercount;
                                                    }

                                                    //END
                                                    decimal discountamount = fare.fares[0].passengerFares[0].discountedFare;

                                                    int servicecharge = fare.fares[0].passengerFares[0].serviceCharges.Count;
                                                    decimal finalamount = 0;
                                                    for (int k = 1; k < servicecharge; k++)
                                                    {

                                                        decimal amount = fare.fares[0].passengerFares[0].serviceCharges[k].amount;
                                                        finalamount += amount;

                                                    }
                                                    //TempData["fareTotalsum"] = fareTotalsum;

                                                    decimal taxamount = finalamount;
                                                    AkasafareIndividual.taxamount = taxamount;
                                                    AkasafareIndividual.faretotal = fareAmount;
                                                    AkasafareIndividual.discountamount = discountamount;
                                                    AkasafareIndividual.passengertype = passengertype;
                                                    AkasafareIndividual.fareKey = fareAvailabilityKey;
                                                    AkasafareIndividual.procuctclass = procuctclass;
                                                    AkasafareIndividualsList.Add(AkasafareIndividual);

                                                }

                                                var expandoconverter = new ExpandoObjectConverter();
                                                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(uniqueJourney.ToString(), expandoconverter);
                                                string jsonresult = JsonConvert.SerializeObject(obj);
                                                //to do
                                                _SimpleAvailibilityaAddResponceobj = JsonConvert.DeserializeObject<SimpleAvailibilityaAddResponce>(jsonresult);

                                                _SimpleAvailibilityaAddResponceobj.designator = AkasaDesignatorobj;
                                                _SimpleAvailibilityaAddResponceobj.segments = Segmentobjlist;
                                                _SimpleAvailibilityaAddResponceobj.arrivalTerminal = arrivalTerminal;
                                                _SimpleAvailibilityaAddResponceobj.departureTerminal = departureTerminal;
                                                _SimpleAvailibilityaAddResponceobj.bookingdate = bookingdate;
                                                _SimpleAvailibilityaAddResponceobj.fareTotalsum = fareTotalsum;
                                                _SimpleAvailibilityaAddResponceobj.journeyKey = journeyKey;
                                                _SimpleAvailibilityaAddResponceobj.faresIndividual = AkasafareIndividualsList;
                                                //_SimpleAvailibilityaAddResponceobj.uniqueId = i;
                                                _SimpleAvailibilityaAddResponceobj.Airline = Airlines.AkasaAir;
                                                _SimpleAvailibilityaAddResponceobj.uniqueId = uniqueidx;
                                                if (_SimpleAvailibilityaAddResponceobj.fareTotalsum <= 0 || stops >=2)
                                                    continue;
                                                uniqueidx++;
                                                //SimpleAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                                                SimpleAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                                            }
                                        }
                                    }

                                }
                            }

                        }
                        #endregion
                        #region spicejet
                        List<SimpleAvailibilityaAddResponce> SpiceJetAvailibilityaAddResponcelist = new List<SimpleAvailibilityaAddResponce>();
                        //Logon 
                        #region Logon
                        Spicejet._login objSpicejet_ = new Spicejet._login();
                        SpicejetSessionManager_.LogonResponse _SpicejetlogonResponseobj = await objSpicejet_.Login(SameAirlineRT, "SpicejetOneWay", AppUrlConstant.BaseURL);
                        #endregion
                        //GetAvailability
                        #region GetAvailability
                        SpicejetBookingManager_.GetAvailabilityVer2Response _getAvailabilityVer2Response = null;
                        SpicejetBookingManager_.GetAvailabilityVer2Response _getAvailabilityRS = null;
                        OnionArchitectureAPI.Services.Spicejet._GetAvailability objspicejetgetAvail_ = new OnionArchitectureAPI.Services.Spicejet._GetAvailability(httpContextAccessorInstance);
                        if (_SpicejetlogonResponseobj != null)
                        {
                            _getAvailabilityVer2Response = await objspicejetgetAvail_.GetTripAvailability(_GetfligthModel, _SpicejetlogonResponseobj, TotalCount, adultcount, childcount, infantcount, flightclass, SameAirlineRT, "SpicejetOneWay");
                            int count1 = 0;
                            if (_getAvailabilityVer2Response != null && _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0].Length > 0)
                            {
                                count1 = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys.Length;
                            }
                            for (int i = 0; i < count1; i++)
                            {

                                string _journeysellkey = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;

                                _SimpleAvailibilityaAddResponceobj = new SimpleAvailibilityaAddResponce();
                                string journeyKey = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                                Designator Designatorobj = new Designator();
                                Designatorobj.origin = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                                Designatorobj.destination = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;


                                string journeykey = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey.ToString();
                                string departureTime = Regex.Match(journeykey, @Designatorobj.origin + @"[\s\S]*?~(?<STD>[\s\S]*?)~").Groups["STD"].Value.Trim();
                                string arrivalTime = Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                                Designatorobj.Arrival = Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();

                                Designatorobj.departure = DateTime.ParseExact(departureTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);// Convert.ToDateTime(departureTime);
                                Designatorobj.arrival = DateTime.ParseExact(arrivalTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);// Convert.ToDateTime(arrivalTime);
                                DateTime SarrivalDateTime = DateTime.ParseExact(Designatorobj.Arrival, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
                                Designatorobj.ArrivalDate = SarrivalDateTime.ToString("yyyy-MM-dd");
                                Designatorobj.ArrivalTime = SarrivalDateTime.ToString("HH:mm:ss");
                                TimeSpan TimeDiff = Designatorobj.arrival - Designatorobj.departure;
                                TimeSpan timeSpan = TimeSpan.Parse(TimeDiff.ToString());
                                if ((int)timeSpan.Minutes == 0)
                                    formatTime = $"{(int)timeSpan.TotalHours} h";
                                else
                                    formatTime = $"{(int)timeSpan.TotalHours} h {(int)timeSpan.Minutes} m";
                                Designatorobj.formatTime = timeSpan;
                                //vivek
                                //Designatorobj.SetformatTime = formatTime;
                                string queryorigin = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                                origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                                Designatorobj.origin = origin;
                                string querydestination = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;
                                destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                Designatorobj.destination = destination1;

                                var segmentscount = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment.Length;
                                List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();
                                List<FareIndividual> fareIndividualsList = new List<FareIndividual>();
                                List<FareIndividual> fareIndividualsconnectedList = new List<FareIndividual>();
                                decimal discountamount = 0M;// JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].discountedFare;
                                decimal finalamount = 0;
                                decimal taxamount = 0M;
                                for (int l = 0; l < segmentscount; l++)
                                {
                                    //Designatorobj.departure = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STD;
                                    //Designatorobj.arrival = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STA;


                                    DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                    Designator SegmentDesignatorobj = new Designator();
                                    SegmentDesignatorobj.origin = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].DepartureStation;
                                    SegmentDesignatorobj.destination = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].ArrivalStation; ;

                                    //queryorigin = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].DepartureStation;
                                    //origin = Citynamelist.GetAllCityData().Where(x => x.cityCode == queryorigin).SingleOrDefault().cityName;
                                    //SegmentDesignatorobj.origin = origin;
                                    //querydestination = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].ArrivalStation;
                                    //destination1 = Citynamelist.GetAllCityData().Where(x => x.cityCode == querydestination).SingleOrDefault().cityName;
                                    //SegmentDesignatorobj.destination = destination1;

                                    SegmentDesignatorobj.departure = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STD;
                                    SegmentDesignatorobj.arrival = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STA;
                                    Segmentobj.designator = SegmentDesignatorobj;
                                    Identifier Identifier = new Identifier();
                                    Identifier.identifier = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].FlightDesignator.FlightNumber; ;
                                    Identifier.carrierCode = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].FlightDesignator.CarrierCode;
                                    Segmentobj.identifier = Identifier;

                                    int legscount = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs.Length;
                                    List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();

                                    for (int m = 0; m < legscount; m++)
                                    {
                                        DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                        Designator legdesignatorobj = new Designator();
                                        legdesignatorobj.origin = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].DepartureStation; ;
                                        legdesignatorobj.destination = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].ArrivalStation; legdesignatorobj.departure = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].STD;
                                        legdesignatorobj.arrival = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].STA;
                                        Legobj.designator = legdesignatorobj;
                                        //Legobj.legKey = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l].legs[m].legKey;
                                        //Legobj.flightReference = JsonObj.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l].legs[m].flightReference;
                                        Leglist.Add(Legobj);

                                        DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                        LegInfo.arrivalTerminal = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.ArrivalTerminal;
                                        LegInfo.departureTerminal = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.DepartureTerminal;
                                        LegInfo.arrivalTime = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.PaxSTA;
                                        LegInfo.departureTime = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.PaxSTD;
                                        var arrivalTerminal = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.ArrivalTerminal;
                                        var departureTerminal = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.DepartureTerminal;

                                        Legobj.legInfo = LegInfo;


                                        _SimpleAvailibilityaAddResponceobj.arrivalTerminal = arrivalTerminal;

                                        _SimpleAvailibilityaAddResponceobj.departureTerminal = departureTerminal;

                                    }
                                    Segmentobj.legs = Leglist;
                                    Segmentobjlist.Add(Segmentobj);
                                    //decimal[] ViewPrice = new decimal[_getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares.Length];
                                    FareIndividual fareIndividual = new FareIndividual();
                                    for (int k2 = 0; k2 < _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares.Length; k2++)
                                    {

                                        string fareindex = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares[k2].FareIndex.ToString();

                                        #region fare
                                        int FareCount = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares.Length;

                                        if (FareCount > 0)
                                        {
                                            //fareIndividualsList = new List<FareIndividual>();

                                            try
                                            {
                                                for (int j = 0; j < FareCount; j++)
                                                {
                                                    if (fareindex == j.ToString())
                                                    {

                                                        fareIndividual = new FareIndividual();
                                                        string _fareSellkey = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                        string fareAvailabilityKey = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                        string fareAvailabilityKeyhead = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                        //var fareAvilableCount = JsonObj.data.faresAvailable[fareAvailabilityKey].fares.Count;
                                                        //var isGoverning = JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].isGoverning;
                                                        var procuctclass = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].ProductClass;
                                                        var passengertype = "";
                                                        decimal fareAmount = 0.0M;
                                                        int servicecharge = 0;
                                                        servicecharge = 0;
                                                        if (_getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].PaxFares.Length > 0)
                                                        {
                                                            passengertype = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].PaxType;
                                                            fareAmount = Math.Round(_getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[0].Amount, 0);
                                                            fareTotalsum = Math.Round(_getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[0].Amount, 0);
                                                            servicecharge = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges.Length;
                                                        }
                                                        else
                                                        {
                                                            continue;
                                                        }

                                                        discountamount = 0M;// JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].discountedFare;

                                                        finalamount = 0;
                                                        taxamount = 0M;
                                                        //for (int k = 1; k < servicecharge; k++) // one way
                                                        for (int k = 0; k < servicecharge; k++)
                                                        {
                                                            if (k > 0)
                                                            {
                                                                taxamount = _getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[k].Amount;
                                                                finalamount += taxamount;
                                                            }

                                                        }
                                                        //ViewPrice[k2] = fareTotalsum+ taxamount;
                                                        taxamount = finalamount;
                                                        fareIndividual.taxamount = taxamount;
                                                        fareIndividual.faretotal = fareAmount + taxamount;
                                                        fareIndividual.discountamount = discountamount;
                                                        fareIndividual.passengertype = passengertype;
                                                        fareIndividual.fareKey = fareAvailabilityKey;
                                                        fareIndividual.procuctclass = procuctclass;
                                                        if (l > 0)
                                                        {
                                                            fareIndividualsconnectedList.Add(fareIndividual);
                                                        }
                                                        else
                                                        {
                                                            fareIndividualsList.Add(fareIndividual);

                                                        }
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                    }
                                }

                                if (segmentscount > 1)
                                {

                                    for (int i1 = 0; i1 < fareIndividualsList.Count; i1++)
                                    {
                                        for (int i2 = 0; i2 < fareIndividualsconnectedList.Count; i2++)
                                        {
                                            if (fareIndividualsconnectedList[i2].procuctclass.Equals(fareIndividualsList[i1].procuctclass))
                                            {
                                                fareIndividualsList[i1].fareKey += "^" + fareIndividualsconnectedList[i2].fareKey;
                                                fareIndividualsList[i1].faretotal += fareIndividualsconnectedList[i2].faretotal;
                                            }

                                        }

                                    }
                                    fareIndividualsList.RemoveAll(x => !x.fareKey.Contains("^"));
                                    #endregion
                                }

                                fareIndividualsconnectedList = fareIndividualsList;

                                //fareIndividualsList.Add(fareIndividual);

                                //}
                                //#endregion


                                int StopCounter = 0;
                                if (Segmentobjlist.Count == 1)
                                {
                                    if (Segmentobjlist[0].legs.Count >= 1)
                                        StopCounter = Segmentobjlist[0].legs.Count;
                                }
                                else
                                    StopCounter = Segmentobjlist.Count;


                                fareTotalsum = 0;
                                //todo Viewprice
                                decimal[] ViewPriceNew = new decimal[fareIndividualsconnectedList.Count];
                                for (int d = 0; d < fareIndividualsconnectedList.Count; d++)
                                {
                                    ViewPriceNew[d] = fareIndividualsconnectedList[d].faretotal;

                                }
                                Array.Sort(ViewPriceNew);
                                if (ViewPriceNew.Length > 0 && ViewPriceNew[0] > 0)
                                {
                                    fareTotalsum = ViewPriceNew[0];
                                }
                                //

                                //Legobj.designator = Designatorobj;
                                _SimpleAvailibilityaAddResponceobj.stops = StopCounter - 1;
                                _SimpleAvailibilityaAddResponceobj.designator = Designatorobj;
                                _SimpleAvailibilityaAddResponceobj.segments = Segmentobjlist;


                                var bookingdate = "2023-12-10T00:00:00";
                                _SimpleAvailibilityaAddResponceobj.bookingdate = Convert.ToDateTime(_getAvailabilityVer2Response.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureDate).ToString("dddd, dd MMM yyyy");
                                _SimpleAvailibilityaAddResponceobj.fareTotalsum = Math.Round(fareTotalsum, 0);

                                _SimpleAvailibilityaAddResponceobj.journeyKey = journeyKey;
                                _SimpleAvailibilityaAddResponceobj.faresIndividual = fareIndividualsconnectedList;// fareIndividualsList;
                                _SimpleAvailibilityaAddResponceobj.uniqueId = uniqueidx;
                                _SimpleAvailibilityaAddResponceobj.Airline = Airlines.Spicejet;
                                if (_SimpleAvailibilityaAddResponceobj.fareTotalsum <= 0)
                                    continue;
                                uniqueidx++;
                                SpiceJetAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                                SimpleAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                            }
                            //string str1 = JsonConvert.SerializeObject(_getAvailabilityVer2Response);
                            //logs.WriteLogs("RequestSpicejetBind: " + JsonConvert.SerializeObject(_getAvailabilityRQ) + "\n\n Response: " + JsonConvert.SerializeObject(_getAvailabilityVer2Response), "GetAvailability", "SpicejetOneWay");
                        }
                        #endregion
                        #endregion
                    }
                    #region Indigo
                    List<SimpleAvailibilityaAddResponce> IndigoAvailibilityaAddResponcelist = new List<SimpleAvailibilityaAddResponce>();
                    //Logon 
                    #region Logon
                    _login obj_ = new _login();
                    IndigoSessionmanager_.LogonResponse _IndigologonResponseobj = await obj_.Login(SameAirlineRT, "IndigooneWay", AppUrlConstant.BaseURL);
                    #endregion
                    //.GetAvailability
                    #region GetAvailability
                    TempData["origin"] = _GetfligthModel.origin;
                    TempData["destination"] = _GetfligthModel.destination;
                    httpContextAccessorInstance = new HttpContextAccessor();
                    OnionArchitectureAPI.Services.Indigo._GetAvailability objgetAvail_ = new OnionArchitectureAPI.Services.Indigo._GetAvailability(httpContextAccessorInstance);
                    IndigoBookingManager_.GetAvailabilityVer2Response _IndigoAvailabilityResponseobj = null;
                    string str2Return = string.Empty;
                    int count2 = 0;
                    if (_IndigologonResponseobj != null)
                    {
                        _IndigoAvailabilityResponseobj = await objgetAvail_.GetTripAvailability(_GetfligthModel, _IndigologonResponseobj, TotalCount, adultcount, childcount, infantcount, flightclass, SameAirlineRT, "IndigoOneWay");
                        count2 = 0;
                        if (_IndigoAvailabilityResponseobj != null && _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0].Length > 0)
                        {
                            count2 = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys.Length;
                        }
                        for (int i = 0; i < count2; i++)
                        {
                            string _journeysellkey = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                            _SimpleAvailibilityaAddResponceobj = new SimpleAvailibilityaAddResponce();
                            string journeyKey = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                            Designator Designatorobj = new Designator();

                            Designatorobj.origin = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                            Designatorobj.destination = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;
                            if (string.IsNullOrEmpty(Designatorobj.destination))
                                Designatorobj.destination = _GetfligthModel.destination;
                            string journeykey = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey.ToString();
                            string departureTime = Regex.Match(journeykey, @Designatorobj.origin + @"[\s\S]*?~(?<STD>[\s\S]*?)~").Groups["STD"].Value.Trim();
                            string arrivalTime = Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                            Designatorobj.departure = DateTime.ParseExact(departureTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(departureTime);
                            Designatorobj.arrival = DateTime.ParseExact(arrivalTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(arrivalTime);
                            Designatorobj.Arrival = Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                            DateTime IarrivalDateTime = DateTime.ParseExact(Designatorobj.Arrival, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
                            Designatorobj.ArrivalDate = IarrivalDateTime.ToString("yyyy-MM-dd");
                            Designatorobj.ArrivalTime = IarrivalDateTime.ToString("HH:mm:ss");
                            TimeSpan TimeDifference = Designatorobj.arrival - Designatorobj.departure;
                            TimeSpan timeSpan = TimeSpan.Parse(TimeDifference.ToString());
                            if ((int)timeSpan.Minutes == 0)
                                formatTime = $"{(int)timeSpan.TotalHours} h";
                            else
                                formatTime = $"{(int)timeSpan.TotalHours} h {(int)timeSpan.Minutes} m";
                            Designatorobj.formatTime = timeSpan;
                            //vivek
                            //Designatorobj.SetformatTime = formatTime;
                            string queryorigin = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                            origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                            Designatorobj.origin = origin;
                            string querydestination = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;
                            if (string.IsNullOrEmpty(querydestination))
                                querydestination = _GetfligthModel.destination;

                            destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                            Designatorobj.destination = destination1;

                            var segmentscount = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment.Length;
                            List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();
                            List<FareIndividual> fareIndividualsList = new List<FareIndividual>();
                            List<FareIndividual> fareIndividualsconnectedList = new List<FareIndividual>();
                            decimal discountamount = 0M;
                            decimal finalamount = 0;
                            decimal taxamount = 0M;
                            int IndoStopcounter = 0;
                            for (int l = 0; l < segmentscount; l++)
                            {
                                DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                Designator SegmentDesignatorobj = new Designator();
                                SegmentDesignatorobj.origin = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].DepartureStation;
                                SegmentDesignatorobj.destination = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].ArrivalStation; ;

                                SegmentDesignatorobj.departure = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STD;
                                SegmentDesignatorobj.arrival = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STA;
                                Segmentobj.designator = SegmentDesignatorobj;
                                Identifier Identifier = new Identifier();
                                Identifier.identifier = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].FlightDesignator.FlightNumber; ;
                                //if (Identifier.identifier == "6163")
                                //{

                                //}
                                Identifier.carrierCode = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].FlightDesignator.CarrierCode;
                                Segmentobj.identifier = Identifier;
                                int legscount = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs.Length;
                                List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();
                                for (int m = 0; m < legscount; m++)
                                {
                                    DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                    Designator legdesignatorobj = new Designator();
                                    legdesignatorobj.origin = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].DepartureStation; ;
                                    legdesignatorobj.destination = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].ArrivalStation;
                                    legdesignatorobj.departure = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].STD;
                                    legdesignatorobj.arrival = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].STA;
                                    Legobj.designator = legdesignatorobj;
                                    Leglist.Add(Legobj);
                                    DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                    LegInfo.arrivalTerminal = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.ArrivalTerminal;
                                    LegInfo.departureTerminal = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.DepartureTerminal;
                                    LegInfo.arrivalTime = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.PaxSTA;
                                    LegInfo.departureTime = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.PaxSTD;
                                    var arrivalTerminal = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.ArrivalTerminal;
                                    var departureTerminal = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.DepartureTerminal;
                                    Legobj.legInfo = LegInfo;
                                    _SimpleAvailibilityaAddResponceobj.arrivalTerminal = arrivalTerminal;
                                    _SimpleAvailibilityaAddResponceobj.departureTerminal = departureTerminal;
                                }
                                IndoStopcounter += legscount;
                                Segmentobj.legs = Leglist;
                                Segmentobjlist.Add(Segmentobj);
                                FareIndividual fareIndividual = new FareIndividual();
                                for (int k2 = 0; k2 < _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares.Length; k2++)
                                {
                                    string fareindex = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares[k2].FareIndex.ToString();
                                    #region fare
                                    int FareCount = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares.Length;
                                    if (FareCount > 0)
                                    {
                                        try
                                        {
                                            for (int j = 0; j < FareCount; j++)
                                            {
                                                if (fareindex == j.ToString())
                                                {
                                                    fareIndividual = new FareIndividual();
                                                    string _fareSellkey = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    string fareAvailabilityKey = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    string fareAvailabilityKeyhead = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    var procuctclass = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].ProductClass;
                                                    var passengertype = "";
                                                    decimal fareAmount = 0.0M;
                                                    int servicecharge = 0;
                                                    servicecharge = 0;
                                                    if (_IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].PaxFares.Length > 0)
                                                    {
                                                        passengertype = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].PaxType;
                                                        fareAmount = Math.Round(_IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[0].Amount, 0);
                                                        fareTotalsum = Math.Round(_IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[0].Amount, 0);
                                                        servicecharge = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges.Length;
                                                    }
                                                    else
                                                    {
                                                        //continue;
                                                    }
                                                    discountamount = 0M;// JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].discountedFare;
                                                    finalamount = 0;
                                                    for (int k = 0; k < servicecharge; k++)
                                                    {
                                                        if (k > 0)
                                                        {
                                                            taxamount = _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[k].Amount;
                                                            finalamount += taxamount;
                                                        }
                                                    }
                                                    taxamount = finalamount;
                                                    fareIndividual.taxamount = taxamount;
                                                    fareIndividual.faretotal = fareAmount + taxamount;
                                                    fareIndividual.discountamount = discountamount;
                                                    fareIndividual.passengertype = passengertype;
                                                    fareIndividual.fareKey = fareAvailabilityKey;
                                                    fareIndividual.procuctclass = procuctclass;

                                                    if (l > 0)
                                                    {
                                                        fareIndividualsconnectedList.Add(fareIndividual);
                                                    }
                                                    else
                                                    {
                                                        fareIndividualsList.Add(fareIndividual);

                                                    }
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                    }
                                }
                            }
                            //fareIndividualsconnectedList = new List<FareIndividual>();
                            if (segmentscount > 1)
                            {
                                for (int i1 = 0; i1 < fareIndividualsList.Count; i1++)
                                {
                                    for (int i2 = 0; i2 < fareIndividualsconnectedList.Count; i2++)
                                    {
                                        if (fareIndividualsconnectedList[i2].procuctclass.Equals(fareIndividualsList[i1].procuctclass) && i2 == i1)
                                        {
                                            fareIndividualsList[i1].fareKey += "^" + fareIndividualsconnectedList[i2].fareKey;
                                            fareIndividualsList[i1].faretotal += fareIndividualsconnectedList[i2].faretotal;
                                        }
                                        else
                                            continue;
                                    }
                                }
                                #endregion
                                fareIndividualsList.RemoveAll(x => !x.fareKey.Contains("^"));
                            }
                            fareIndividualsconnectedList = fareIndividualsList;
                            var duplicates = fareIndividualsconnectedList.GroupBy(x => x.procuctclass).Where(g => g.Count() > 1).SelectMany(g => g).ToHashSet();

                            // Remove all items that are duplicates
                            fareIndividualsconnectedList = fareIndividualsconnectedList.Where(item => !duplicates.Contains(item)).ToList();

                            //var duplicatesnonstop = fareIndividualsList.GroupBy(x => x.procuctclass).Where(g => g.Count() > 1).SelectMany(g => g).ToHashSet();

                            //// Remove all items that are duplicates
                            //fareIndividualsList = fareIndividualsList.Where(item => !duplicatesnonstop.Contains(item)).ToList();
                            fareTotalsum = 0;
                            //todo Viewprice
                            decimal[] ViewPriceNew = new decimal[fareIndividualsconnectedList.Count];
                            for (int d = 0; d < fareIndividualsconnectedList.Count; d++)
                            {
                                ViewPriceNew[d] = fareIndividualsconnectedList[d].faretotal;

                            }
                            Array.Sort(ViewPriceNew);
                            if (ViewPriceNew.Length > 0 && ViewPriceNew[0] > 0)
                            {
                                fareTotalsum = ViewPriceNew[0];
                            }
                            _SimpleAvailibilityaAddResponceobj.stops = IndoStopcounter - 1;
                            _SimpleAvailibilityaAddResponceobj.designator = Designatorobj;
                            _SimpleAvailibilityaAddResponceobj.segments = Segmentobjlist;
                            DateTime currentDate = DateTime.Now;
                            var bookingdate1 = currentDate; //"2023-12-10T00:00:00";
                            _SimpleAvailibilityaAddResponceobj.bookingdate = Convert.ToDateTime(_IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureDate).ToString("dddd, dd MMM yyyy");
                            _SimpleAvailibilityaAddResponceobj.fareTotalsum = Math.Round(fareTotalsum, 0);
                            _SimpleAvailibilityaAddResponceobj.journeyKey = journeyKey;
                            _SimpleAvailibilityaAddResponceobj.faresIndividual = fareIndividualsconnectedList;// fareIndividualsList;
                            _SimpleAvailibilityaAddResponceobj.uniqueId = uniqueidx;
                            _SimpleAvailibilityaAddResponceobj.Airline = Airlines.Indigo;
                            if (_SimpleAvailibilityaAddResponceobj.fareTotalsum <= 0)
                                continue;
                            uniqueidx++;
                            SimpleAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                        }
                        //str2Return = string.Empty;
                        //if (_IndigoAvailabilityResponseobj != null)
                        //{
                        // str2Return = JsonConvert.SerializeObject(_IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response);
                        //}
                        //OneWayTrip
                        HttpContext.Session.SetString("IndigoSignature", JsonConvert.SerializeObject(_IndigologonResponseobj.Signature));
                    }
                    #endregion

                    #endregion
                    #region GDS
                    // string _testURL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/AirService";
                    string _testURL = AppUrlConstant.GDSURL;
                    string _targetBranch = string.Empty;
                    string _userName = string.Empty;
                    string _password = string.Empty;
                    string res = string.Empty;
                    StringBuilder sbReq = null;
                    //GDS Login
                    _credentials _CredentialsGDS = new _credentials();
                    if (response.IsSuccessStatusCode)
                    {
                        //var results = response.Content.ReadAsStringAsync().Result;
                        //var JsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);
                        await _credentialService.PopulateCredentialsAsync(response, _CredentialsGDS, 5);

                    }

                    sbReq = new StringBuilder();
                    Guid newGuid = Guid.NewGuid();
                    httpContextAccessorInstance = new HttpContextAccessor();
                    TravelPort _objAvail = null;
                    _objAvail = new TravelPort(httpContextAccessorInstance);
                    res = _objAvail.GetAvailabilty(_testURL, sbReq, _objAvail, _GetfligthModel, newGuid.ToString(), _CredentialsGDS.domain, _CredentialsGDS.username, _CredentialsGDS.password, flightclass, SameAirlineRT, "GDSOneWay");
                    TempData["origin"] = _GetfligthModel.origin;
                    TempData["destination"] = _GetfligthModel.destination;
                    TravelPortParsing _objP = new TravelPortParsing();
                    List<GDSResModel.Segment> getAvailRes = new List<GDSResModel.Segment>();
                    if (res != null && !res.Contains("Bad Request") && !res.Contains("Internal Server Error"))
                    {
                        getAvailRes = _objP.ParseLowFareSearchRsp2(res, "OneWay", Convert.ToDateTime(_GetfligthModel.beginDate));
                    }
                    //var getAvailRes = _objP.ParseLowFareSearchRsp2(res, "OneWay", Convert.ToDateTime(_GetfligthModel.beginDate));
                    //string test = JsonConvert.SerializeObject(getAvailRes, Formatting.Indented);
                    //logs.WriteLogs("\n Response: " + test, "gdsLowfaremodel", "GDSOneWay");

                    // to do
                    count2 = 0;
                    if (getAvailRes != null && getAvailRes.Count > 0)
                    {
                        count2 = getAvailRes.Count;
                    }
                    for (int i = 0; i < count2; i++)
                    {
                        for (int k = 0; k < getAvailRes[i].Bonds.Count; k++)
                        {
                            string _journeysellkey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                            _SimpleAvailibilityaAddResponceobj = new SimpleAvailibilityaAddResponce();
                            string journeyKey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                            Designator Designatorobj = new Designator();

                            if (getAvailRes[i].Bonds[k].BoundType.ToLower() == "outbound")
                            {
                                try
                                {
                                    List<SimpleAvailibilityaAddResponce> matchingItineraries1 = SimpleAvailibilityaAddResponcelist.Where(it => it.Identifier == getAvailRes[i].Bonds[k].FlightNumber).ToList();
                                    //if (matchingItineraries1.Count>0 && matchingItineraries1[0].Identifier=="811")
                                    //{

                                    //}
                                    //if (matchingItineraries1.Count == 1)
                                    //continue;
                                }
                                catch (Exception ex)
                                {

                                }
                                Designatorobj.origin = getAvailRes[i].Bonds[k].Legs[0].Origin;//_IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                                Designatorobj.destination = getAvailRes[i].Bonds[k].Legs[0].Destination;// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;
                                string journeykey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey.ToString();
                                string departureTime = getAvailRes[i].Bonds[k].Legs[0].DepartureTime;// Regex.Match(journeykey, @Designatorobj.origin + @"[\s\S]*?~(?<STD>[\s\S]*?)~").Groups["STD"].Value.Trim();
                                string arrivalTime = getAvailRes[i].Bonds[k].Legs[0].ArrivalTime;// ; Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                                Designatorobj.departure = DateTimeOffset.Parse(getAvailRes[i].Bonds[k].Legs[0].DepartureTime).DateTime; //Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[0].DepartureTime); // DateTime.ParseExact(departureTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(departureTime);

                                if (getAvailRes[i].Bonds[k].Legs.Count == 3)
                                {
                                    Designatorobj.arrival = DateTimeOffset.Parse(getAvailRes[i].Bonds[k].Legs[2].ArrivalTime).DateTime; //Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[2].ArrivalTime);
                                }
                                else if (getAvailRes[i].Bonds[k].Legs.Count == 2)
                                {
                                    Designatorobj.arrival = DateTimeOffset.Parse(getAvailRes[i].Bonds[k].Legs[1].ArrivalTime).DateTime; //Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[1].ArrivalTime);
                                }
                                else
                                {
                                    Designatorobj.arrival = DateTimeOffset.Parse(getAvailRes[i].Bonds[k].Legs[0].ArrivalTime).DateTime;//Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[0].ArrivalTime);
                                }
                                //Designatorobj.arrival = Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[0].ArrivalTime); // DateTime.ParseExact(arrivalTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(arrivalTime);
                                Designatorobj.Arrival = "";// Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                                TimeSpan TimeDifference = Designatorobj.arrival - Designatorobj.departure;
                                TimeSpan timeSpan = TimeSpan.Parse(TimeDifference.ToString());
                                if (timeSpan.Minutes == 0)
                                    formatTime = $"{(int)timeSpan.TotalHours} h";
                                else
                                    formatTime = $"{(int)timeSpan.TotalHours} h {timeSpan.Minutes} m";
                                Designatorobj.formatTime = timeSpan;
                                //vivek
                                //Designatorobj.SetformatTime = formatTime;
                                string queryorigin = getAvailRes[i].Bonds[k].Legs[0].Origin;
                                origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                                Designatorobj.origin = origin;
                                string querydestination = string.Empty;
                                if (getAvailRes[i].Bonds[k].Legs.Count == 3)
                                {
                                    querydestination = getAvailRes[i].Bonds[k].Legs[2].Destination;
                                    destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                    Designatorobj.destination = destination1;
                                }
                                else
                                {
                                    if (getAvailRes[i].Bonds[k].Legs.Count > 1)
                                    {
                                        querydestination = getAvailRes[i].Bonds[k].Legs[1].Destination;
                                        destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                        Designatorobj.destination = destination1;

                                    }
                                    else
                                    {
                                        querydestination = getAvailRes[i].Bonds[k].Legs[0].Destination;
                                        destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                        Designatorobj.destination = destination1;
                                    }
                                }

                                var segmentscount = getAvailRes[i].Bonds[k].Legs.Count;
                                List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();
                                List<FareIndividual> fareIndividualsList = new List<FareIndividual>();
                                List<FareIndividual> fareIndividualsconnectedList = new List<FareIndividual>();
                                decimal discountamount = 0M;
                                decimal finalamount = 0;
                                decimal taxamount = 0M;
                                int IndoStopcounter = 0;
                                for (int l = 0; l < segmentscount; l++)
                                {
                                    DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                    Designator SegmentDesignatorobj = new Designator();
                                    SegmentDesignatorobj.origin = getAvailRes[i].Bonds[k].Legs[l].Origin;
                                    SegmentDesignatorobj.destination = getAvailRes[i].Bonds[k].Legs[l].Destination;

                                    SegmentDesignatorobj.departure = Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[l].DepartureTime);
                                    SegmentDesignatorobj.arrival = Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[l].ArrivalTime);

                                    SegmentDesignatorobj._DepartureDate = getAvailRes[i].Bonds[k].Legs[l].DepartureTime;
                                    SegmentDesignatorobj._AvailabilitySource = getAvailRes[i].Bonds[k].Legs[l]._AvailabilitySource;
                                    SegmentDesignatorobj._AvailabilityDisplayType = getAvailRes[i].Bonds[k].Legs[l]._AvailabilityDisplayType;
                                    SegmentDesignatorobj._FlightTime = getAvailRes[i].Bonds[k].Legs[l].Duration;
                                    SegmentDesignatorobj._Equipment = getAvailRes[i].Bonds[k].Legs[l]._Equipment;
                                    SegmentDesignatorobj._Distance = getAvailRes[i].Bonds[k].Legs[l]._Distance;
                                    SegmentDesignatorobj._ArrivalDate = getAvailRes[i].Bonds[k].Legs[l]._ArrivalDate;
                                    SegmentDesignatorobj._Group = getAvailRes[i].Bonds[k].Legs[l].Group;
                                    SegmentDesignatorobj._ProviderCode = getAvailRes[i].Bonds[k].Legs[l].ProviderCode;
                                    SegmentDesignatorobj._ClassOfService = getAvailRes[i].Bonds[k].Legs[l].FareClassOfService;


                                    Segmentobj.designator = SegmentDesignatorobj;
                                    Identifier Identifier = new Identifier();
                                    Identifier.identifier = getAvailRes[i].Bonds[k].Legs[l].FlightNumber;
                                    //if (Identifier.identifier == "817")
                                    //{
                                    //var t = SimpleAvailibilityaAddResponcelist[0].segments[0].identifier.identifier.ToString();
                                    //}
                                    Identifier.carrierCode = getAvailRes[i].Bonds[k].Legs[l].CarrierCode;
                                    //to do && it.segments[0].identifier.carrierCode == getAvailRes[i].Bonds[k].Legs[0].CarrierCode
                                    Segmentobj.identifier = Identifier;
                                    int legscount = 1;// getAvailRes[i].Bonds[k].Legs.Count;
                                    List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();
                                    for (int m = 0; m < legscount; m++)
                                    {
                                        DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                        Designator legdesignatorobj = new Designator();
                                        legdesignatorobj.origin = getAvailRes[i].Bonds[k].Legs[l].Origin;
                                        legdesignatorobj.destination = getAvailRes[i].Bonds[k].Legs[l].Destination;
                                        legdesignatorobj.departure = Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[l].DepartureTime);
                                        legdesignatorobj.arrival = Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[l].ArrivalTime);
                                        Legobj.designator = legdesignatorobj;

                                        DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                        LegInfo.arrivalTerminal = getAvailRes[i].Bonds[k].Legs[l].ArrivalTerminal;
                                        LegInfo.departureTerminal = getAvailRes[i].Bonds[k].Legs[l].DepartureTerminal;
                                        LegInfo.arrivalTime = Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[l].ArrivalTime);
                                        LegInfo.departureTime = Convert.ToDateTime(getAvailRes[i].Bonds[k].Legs[l].DepartureTime);
                                        var arrivalTerminal = getAvailRes[i].Bonds[k].Legs[l].ArrivalTerminal;
                                        var departureTerminal = getAvailRes[i].Bonds[k].Legs[l].DepartureTerminal;
                                        Legobj.legInfo = LegInfo;
                                        Leglist.Add(Legobj);
                                        _SimpleAvailibilityaAddResponceobj.arrivalTerminal = arrivalTerminal;
                                        _SimpleAvailibilityaAddResponceobj.departureTerminal = departureTerminal;
                                    }

                                    Segmentobj.legs = Leglist;
                                    Segmentobjlist.Add(Segmentobj);
                                    decimal fareAmount = 0.0M;
                                    fareAmount = Math.Round(getAvailRes[i].Fare.PaxFares[0].BasicFare, 0);
                                    FareIndividual fareIndividual = new FareIndividual();
                                    List<GDSResModel.Segment> matchingItineraries = getAvailRes.Where(it => it.Segmentid == getAvailRes[i].Segmentid).ToList();
                                    string s = JsonConvert.SerializeObject(matchingItineraries);
                                    if (matchingItineraries.Count > 0)
                                    {
                                        try
                                        {
                                            for (int j = 0; j < matchingItineraries.Count; j++)
                                            {

                                                fareIndividual = new FareIndividual();
                                                string _fareSellkey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                string fareAvailabilityKey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                string fareAvailabilityKeyhead = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                var procuctclass = matchingItineraries[j].Bonds[k].Legs[l].Branddesc;// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].ProductClass;
                                                fareAvailabilityKey = matchingItineraries[j].Bonds[k].Legs[l]._FareBasisCodeforAirpriceHit;
                                                var passengertype = "";
                                                fareAmount = 0.0M;
                                                int servicecharge = 0;
                                                servicecharge = 0;
                                                passengertype = matchingItineraries[j].Fare.PaxFares[0].PaxType.ToString();
                                                fareAmount = Math.Round(matchingItineraries[j].Fare.PaxFares[0].BasicFare, 0);
                                                fareTotalsum = Math.Round(matchingItineraries[j].Fare.PaxFares[0].BasicFare, 0);
                                                taxamount = Math.Round(matchingItineraries[j].Fare.PaxFares[0].TotalTax, 0);

                                                discountamount = 0M;// JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].discountedFare;
                                                fareIndividual.taxamount = taxamount;
                                                fareIndividual.faretotal = fareAmount + taxamount;
                                                fareIndividual.discountamount = discountamount;
                                                fareIndividual.passengertype = passengertype;
                                                fareIndividual.fareKey = fareAvailabilityKey;
                                                fareIndividual.procuctclass = procuctclass;

                                                if (l > 0)
                                                {
                                                    fareIndividualsconnectedList.Add(fareIndividual);
                                                }
                                                else
                                                {
                                                    fareIndividualsList.Add(fareIndividual);

                                                }

                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                    }
                                    if (getAvailRes[i].Bonds[k].Legs[l].FlightNumber == "573" || getAvailRes[i].Bonds[k].Legs[l].FlightNumber == "545")
                                    {

                                    }
                                    if (string.IsNullOrEmpty(_SimpleAvailibilityaAddResponceobj.Identifier))
                                    {
                                        _SimpleAvailibilityaAddResponceobj.Identifier = getAvailRes[i].Bonds[k].Legs[l].FlightNumber;
                                    }
                                    else
                                    {
                                        _SimpleAvailibilityaAddResponceobj.Identifier += "@" + getAvailRes[i].Bonds[k].Legs[l].FlightNumber;
                                    }
                                    if (string.IsNullOrEmpty(_SimpleAvailibilityaAddResponceobj.SegmentidLeftdata))
                                    {
                                        _SimpleAvailibilityaAddResponceobj.SegmentidLeftdata = getAvailRes[i].Bonds[k].Legs[l].AircraftCode;
                                        _SimpleAvailibilityaAddResponceobj.FareBasisLeftdata = getAvailRes[i].Bonds[k].Legs[l]._FareBasisCodeforAirpriceHit;
                                    }
                                    else
                                    {
                                        _SimpleAvailibilityaAddResponceobj.SegmentidLeftdata += "@" + getAvailRes[i].Bonds[k].Legs[l].AircraftCode;
                                        _SimpleAvailibilityaAddResponceobj.FareBasisLeftdata += "@" + getAvailRes[i].Bonds[k].Legs[l]._FareBasisCodeforAirpriceHit;
                                    }
                                }
                                IndoStopcounter += segmentscount;
                                if (segmentscount > 1)
                                {
                                    for (int i1 = 0; i1 < fareIndividualsList.Count; i1++)
                                    {
                                        for (int i2 = 0; i2 < fareIndividualsconnectedList.Count; i2++)
                                        {
                                            if (fareIndividualsconnectedList[i2].procuctclass != null && fareIndividualsconnectedList[i2].procuctclass.Equals(fareIndividualsList[i1].procuctclass) && i2 == i1)
                                            {
                                                fareIndividualsList[i1].fareKey += "^" + fareIndividualsconnectedList[i2].fareKey;
                                                fareIndividualsList[i1].faretotal = fareIndividualsconnectedList[i2].faretotal;
                                            }
                                            else
                                                continue;
                                        }
                                    }
                                    fareIndividualsList.RemoveAll(x => !x.fareKey.Contains("^"));
                                }
                                #endregion
                                fareIndividualsconnectedList = fareIndividualsList;
                                //int StopCounter = 0;
                                //if (Segmentobjlist.Count == 1)
                                //{
                                //if (Segmentobjlist[0].legs.Count >= 1)
                                //StopCounter = Segmentobjlist[0].legs.Count;
                                //}
                                //else
                                //StopCounter = Segmentobjlist.Count;


                                fareTotalsum = 0;
                                //todo Viewprice
                                decimal[] ViewPriceNew = new decimal[fareIndividualsconnectedList.Count];
                                for (int d = 0; d < fareIndividualsconnectedList.Count; d++)
                                {
                                    ViewPriceNew[d] = fareIndividualsconnectedList[d].faretotal;

                                }
                                Array.Sort(ViewPriceNew);
                                if (ViewPriceNew.Length > 0 && ViewPriceNew[0] > 0)
                                {
                                    fareTotalsum = ViewPriceNew[0];
                                }
                                _SimpleAvailibilityaAddResponceobj.Segmentiddata = getAvailRes[i].Segmentid;
                                _SimpleAvailibilityaAddResponceobj.stops = IndoStopcounter - 1;
                                _SimpleAvailibilityaAddResponceobj.designator = Designatorobj;
                                _SimpleAvailibilityaAddResponceobj.segments = Segmentobjlist;
                                DateTime currentDate = DateTime.Now;
                                var bookingdate1 = currentDate; //"2023-12-10T00:00:00";
                                _SimpleAvailibilityaAddResponceobj.bookingdate = Convert.ToDateTime(Segmentobjlist[0].designator._DepartureDate).ToString("dddd, dd MMM yyyy");
                                //if (_IndigoAvailabilityResponseobj == null)
                                //{
                                //    _SimpleAvailibilityaAddResponceobj.bookingdate = bookingdate1.ToString(); ;
                                //}
                                //else
                                //{
                                //    _SimpleAvailibilityaAddResponceobj.bookingdate = Convert.ToDateTime(_IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureDate).ToString("dddd, dd MMM yyyy");
                                //}

                                _SimpleAvailibilityaAddResponceobj.fareTotalsum = Math.Round(fareTotalsum, 0);
                                _SimpleAvailibilityaAddResponceobj.journeyKey = journeyKey;
                                _SimpleAvailibilityaAddResponceobj.faresIndividual = fareIndividualsconnectedList;// fareIndividualsList;
                                _SimpleAvailibilityaAddResponceobj.uniqueId = uniqueidx;
                                if (_SimpleAvailibilityaAddResponceobj.segments[0].identifier.carrierCode.Equals("UK"))
                                    _SimpleAvailibilityaAddResponceobj.Airline = Airlines.Vistara;
                                else if (_SimpleAvailibilityaAddResponceobj.segments[0].identifier.carrierCode.Equals("AI"))
                                    _SimpleAvailibilityaAddResponceobj.Airline = Airlines.AirIndia;
                                else if (_SimpleAvailibilityaAddResponceobj.segments[0].identifier.carrierCode.Equals("H1"))
                                    _SimpleAvailibilityaAddResponceobj.Airline = Airlines.Hehnair;
                                if (_SimpleAvailibilityaAddResponceobj.fareTotalsum <= 0)
                                    continue;
                                uniqueidx++;
                                //SpiceJetAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                                SimpleAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                            }
                        }
                    }
                    //var x = SimpleAvailibilityaAddResponcelist.Distinct().ToList();
                    //str2Return = string.Empty;
                    //if (getAvailRes != null && getAvailRes.Count > 0)
                    //{
                    //str2Return = JsonConvert.SerializeObject(getAvailRes);
                    //}
                    //OneWayTrip
                    HttpContext.Session.SetString("GDSTraceid", JsonConvert.SerializeObject(newGuid.ToString()));
                    //

                    //#endregion
                    //if (_SimpleAvailabilityobj.beginDate != null && _SimpleAvailabilityobj.endDate != null)
                    if (_GetfligthModel.beginDate != null && _GetfligthModel.endDate != null)
                    {

                        var AdtTypeR = string.Empty;
                        var AdtCountR = 0;
                        var chdtypeR = string.Empty;
                        var chdcountR = 0;
                        var infanttypeR = string.Empty;
                        var infantcountR = 0;

                        uniqueidx = 0;
                        ////Roundtripcode for AirAsia
                        SpicejetSessionManager_.LogonResponse _SpicejetlogonResponseobjR = null;
                        SimpleAvailibilityaAddResponcelistR = new List<SimpleAvailibilityaAddResponce>();
                        _SimpleAvailibilityaAddResponceobjR = new SimpleAvailibilityaAddResponce();

                        DomainLayer.Model.SimpleAvailabilityRequestModel _SimpleAvailabilityobjR = new DomainLayer.Model.SimpleAvailabilityRequestModel();
                        _SimpleAvailabilityobjR.origin = _GetfligthModel.destination;
                        _SimpleAvailabilityobjR.destination = _GetfligthModel.origin;
                        _SimpleAvailabilityobjR.beginDate = _GetfligthModel.endDate;
                        _SimpleAvailabilityobj.endDate = _GetfligthModel.endDate; //"2023-12-20";//_GetfligthModel.endDate;
                        Codessimple _codesR = new Codessimple();



                        List<Typesimple> _typeslistR = new List<Typesimple>();
                        if (_GetfligthModel.passengercount != null)
                        {
                            AdtTypeR = _GetfligthModel.passengercount.adulttype;
                            AdtCountR = _GetfligthModel.passengercount.adultcount;
                            chdtypeR = _GetfligthModel.passengercount.childtype;
                            chdcountR = _GetfligthModel.passengercount.childcount;
                            infanttypeR = _GetfligthModel.passengercount.infanttype;
                            infantcountR = _GetfligthModel.passengercount.infantcount;
                        }
                        else
                        {
                            AdtTypeR = _GetfligthModel.adulttype;
                            AdtCountR = _GetfligthModel.adultcount;
                            chdtypeR = _GetfligthModel.childtype;
                            chdcountR = _GetfligthModel.childcount;
                            infanttypeR = _GetfligthModel.infanttype;
                            infantcountR = _GetfligthModel.infantcount;
                        }

                        if (AdtTypeR == "ADT" && AdtCountR != 0)
                        {
                            Typesimple TypesR = new Typesimple();
                            TypesR.type = AdtType;
                            TypesR.count = AdtCount;

                            _typeslistR.Add(TypesR);
                        }
                        if (chdtypeR == "CHD" && chdcountR != 0)
                        {
                            Typesimple TypesR = new Typesimple();
                            TypesR.type = chdtype;
                            TypesR.count = chdcount;

                            _typeslistR.Add(TypesR);
                        }
                        if (infanttypeR == "INFT" && infantcountR != 0)
                        {
                            Typesimple TypesR = new Typesimple();
                            TypesR.type = infanttype;
                            TypesR.count = infantcount;

                            _typeslistR.Add(TypesR);
                        }


                        Passengerssimple _PassengerssimpleR = new Passengerssimple();
                        _PassengerssimpleR.types = _typeslist;
                        _SimpleAvailabilityobjR.passengers = _Passengerssimple;

                        //_codes.currencyCode = "INR";
                        _SimpleAvailabilityobjR.codes = _codes;
                        _SimpleAvailabilityobjR.sourceOrganization = "";
                        _SimpleAvailabilityobjR.currentSourceOrganization = "";
                        _SimpleAvailabilityobjR.promotionCode = "";
                        string[] sortOptionsR = new string[1];
                        sortOptionsR[0] = "ServiceType";
                        Filters FiltersR = new Filters();
                        if (flightclass == "B")
                        {
                            string[] fareTypesR = new string[1];
                            fareTypesR[0] = "R";
                            string[] productClassesR = new string[1];
                            productClassesR[0] = "VV";
                            FiltersR.fareTypes = fareTypesR;
                            FiltersR.productClasses = productClassesR;
                        }
                        else
                        {
                            string[] fareTypesR = new string[2];
                            fareTypesR[0] = "R";
                            fareTypesR[1] = "M";

                            string[] productClassesR = new string[3];
                            productClassesR[0] = "EC";
                            productClassesR[1] = "HF";
                            productClassesR[2] = "EP";
                            FiltersR.fareTypes = fareTypesR;
                            FiltersR.productClasses = productClassesR;
                        }
                        FiltersR.exclusionType = "Default";
                        FiltersR.loyalty = "MonetaryOnly";
                        FiltersR.includeAllotments = true;
                        FiltersR.connectionType = "Both";
                        FiltersR.compressionType = "CompressByProductClass";
                        FiltersR.sortOptions = sortOptions;
                        FiltersR.maxConnections = 10;
                        _SimpleAvailabilityobjR.filters = FiltersR;
                        _SimpleAvailabilityobjR.taxesAndFees = "Taxes";
                        _SimpleAvailabilityobjR.ssrCollectionsMode = "Leg";
                        _SimpleAvailabilityobjR.numberOfFaresPerJourney = 10;
                        var jsonR = JsonConvert.SerializeObject(_SimpleAvailabilityobjR, Formatting.Indented);
                        //To do
                        var result1s = response.Content.ReadAsStringAsync().Result;
                        var JsonObject = JsonConvert.DeserializeObject<List<_credentials>>(result1s);
                        var _credentialsAirasiaR = new _credentials();

                        //AirAsia round login

                        await _credentialService.PopulateCredentialsAsync(response, _credentialsAirasiaR, 1);

                        login = new airlineLogin();
                        login.credentials = _credentialsAirasiaR;

                        //till here
                        TempData["AirAsiaLogin"] = login.credentials.Image;
                        AirasiaTokan = new AirasiaTokan();
                        AirasialoginRequest = JsonConvert.SerializeObject(login, Formatting.Indented);
                        logs.WriteLogsR(AirasialoginRequest, "1-Tokan_Request", "AirAsiaRT");
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        responce = await client.PostAsJsonAsync(AppUrlConstant.AirasiaTokan, login);

                        if (responce.IsSuccessStatusCode)
                        {
                            var results = responce.Content.ReadAsStringAsync().Result;
                            logs.WriteLogsR(results, "1-Token_Responce", "AirAsiaRT");
                            var JsonObj = JsonConvert.DeserializeObject<dynamic>(results);
                            AirasiaTokan.token = JsonObj.data.token;
                            AirasiaTokan.idleTimeoutInMinutes = JsonObj.data.idleTimeoutInMinutes;
                            //token = ((Newtonsoft.Json.Linq.JValue)value).Value.ToString();
                        }
                        //logs.WriteLogsR("Request: " + AirasialoginRequest + "\n Response: " + JsonConvert.SerializeObject(AirasiaTokan.token), "Logon", "AirAsiaRT");


                        HttpContext.Session.SetString("AirasiaTokanR", JsonConvert.SerializeObject(AirasiaTokan.token));
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AirasiaTokan.token);
                        logs.WriteLogsR(jsonR, "2-SimpleAvailability_Req", "AirAsiaRT");
                        HttpResponseMessage responceR = await client.PostAsJsonAsync(AppUrlConstant.AirasiasearchsimpleR, _SimpleAvailabilityobjR);
                        if (responceR.IsSuccessStatusCode)
                        {
                            var resultsR = responceR.Content.ReadAsStringAsync().Result;
                            logs.WriteLogsR(resultsR, "2-SimpleAvailability_Res", "AirAsiaRT");
                            //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_SimpleAvailabilityobjR) + "Url: " + AppUrlConstant.AirasiasearchsimpleR + "\n Response: " + JsonConvert.SerializeObject(resultsR), "GetAvailability", "AirAsiaRT");
                            var JsonObjR = JsonConvert.DeserializeObject<dynamic>(resultsR);
                            // var value = JsonObj.data.token;
                            //var value = JsonObj.data.results[0].trips[0].date;
                            var oriDes = _GetfligthModel.destination + "|" + _GetfligthModel.origin;
                            TempData["originR"] = _SimpleAvailabilityobjR.origin;
                            TempData["destinationR"] = _SimpleAvailabilityobjR.destination;

                            if (JsonObjR.data.results != null && ((JArray)JsonObjR.data.results).Count > 0)
                            {
                                var finddate = JsonObjR.data.results[0].trips[0].date;
                                var bookingdate = finddate.ToString("dddd, dd MMMM yyyy");



                                int count = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes].Count;
                                TempData["countr"] = count;



                                for (int i = 0; i < JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes].Count; i++)
                                {
                                    var journeyR = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i];
                                    string journeyKey = journeyR.journeyKey;
                                    int stopsR = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].stops;                                   //journeyKey1 = ((Newtonsoft.Json.Linq.JValue)journeyKey).Value.ToString();
                                    var destination = journeyR;
                                    Designator Designatorobj = new Designator();
                                    Designatorobj.origin = journeyR.designator.origin;
                                    Designatorobj.destination = journeyR.designator.destination;
                                    Designatorobj.departure = journeyR.designator.departure;
                                    Designatorobj.arrival = journeyR.designator.arrival;


                                    string queryorigin = journeyR.designator.origin;
                                    origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                                    Designatorobj.origin = origin;
                                    string querydestination = journeyR.designator.destination;
                                    destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                    Designatorobj.destination = destination1;

                                    var segmentscount = journeyR.segments.Count;
                                    List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();

                                    for (int l = 0; l < segmentscount; l++)
                                    {
                                        DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                        Designator SegmentDesignatorobj = new Designator();
                                        var JousegR = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l];
                                        SegmentDesignatorobj.origin = JousegR.designator.origin;
                                        SegmentDesignatorobj.destination = JousegR.designator.destination;
                                        SegmentDesignatorobj.departure = JousegR.designator.departure;
                                        SegmentDesignatorobj.arrival = JousegR.designator.arrival;
                                        Segmentobj.designator = SegmentDesignatorobj;
                                        Identifier Identifier = new Identifier();
                                        Identifier.identifier = JousegR.identifier.identifier;
                                        Identifier.carrierCode = JousegR.identifier.carrierCode;
                                        Segmentobj.identifier = Identifier;

                                        int legscount = JousegR.legs.Count;
                                        List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();
                                        for (int m = 0; m < legscount; m++)
                                        {
                                            DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                            Designator legdesignatorobj = new Designator();
                                            var JouseglegR = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l].legs[m];
                                            legdesignatorobj.origin = JouseglegR.designator.origin;
                                            legdesignatorobj.destination = JouseglegR.designator.destination;
                                            legdesignatorobj.departure = JouseglegR.designator.departure;
                                            legdesignatorobj.arrival = JouseglegR.designator.arrival;
                                            Legobj.designator = legdesignatorobj;
                                            Legobj.legKey = JouseglegR.legKey;
                                            Legobj.flightReference = JouseglegR.flightReference;
                                            Leglist.Add(Legobj);
                                            DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                            LegInfo.arrivalTerminal = JouseglegR.legInfo.arrivalTerminal;
                                            LegInfo.departureTerminal = JouseglegR.legInfo.departureTerminal;
                                            LegInfo.arrivalTime = JouseglegR.legInfo.arrivalTime;
                                            LegInfo.departureTime = JouseglegR.legInfo.departureTime;
                                            Legobj.legInfo = LegInfo;

                                        }
                                        //  Leglist.Add(Legobj);
                                        Segmentobj.legs = Leglist;
                                        Segmentobjlist.Add(Segmentobj);

                                    }


                                    var arrivalTerminal = journeyR.segments[0].legs[0].legInfo.arrivalTerminal;
                                    var departureTerminal = journeyR.segments[0].legs[0].legInfo.departureTerminal;
                                    int FareCount = journeyR.fares.Count;


                                    if (FareCount > 0)
                                    {
                                        List<FareIndividual> fareIndividualsList = new List<FareIndividual>();

                                        for (int j = 0; j < FareCount; j++)
                                        {
                                            //x.data.faresAvailable["Mn5Lfn5JNX5LTlJBMDAwfk5SMDB_fjB_MTN_flheMX5Lfn5JNX5LTlJBMDAwfk5SMDB_fjB_MjB_fiEzNA--"].fares

                                            FareIndividual fareIndividual = new FareIndividual();

                                            string fareAvailabilityKey = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].fares[j].fareAvailabilityKey;
                                            string fareAvailabilityKeyhead = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].fares[0].fareAvailabilityKey;
                                            var fareAvilable = JsonObjR.data.faresAvailable[fareAvailabilityKey];
                                            var fareAvilableCount = fareAvilable.fares.Count;
                                            var isGoverning = fareAvilable.fares[0].isGoverning;
                                            var procuctclass = fareAvilable.fares[0].productClass;
                                            var passengertype = fareAvilable.fares[0].passengerFares[0].passengerType;
                                            fareTotalsum = 0;
                                            var fareAmount = 0;
                                            for (int fc = 0; fc < fareAvilableCount; fc++)
                                            {
                                                int fareH = JsonObjR.data.faresAvailable[fareAvailabilityKeyhead].fares[fc].passengerFares[0].fareAmount;

                                                fareTotalsum += fareH;
                                                int fare = JsonObjR.data.faresAvailable[fareAvailabilityKey].fares[fc].passengerFares[0].fareAmount;
                                                fareAmount = fare + fareAmount;
                                            }
                                            // var fareAmount = JsonObjR.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].fareAmount;



                                            decimal discountamount = fareAvilable.fares[0].passengerFares[0].discountedFare;

                                            int servicecharge = fareAvilable.fares[0].passengerFares[0].serviceCharges.Count;
                                            decimal finalamount = 0;
                                            for (int k = 1; k < servicecharge; k++)
                                            {

                                                decimal amount = fareAvilable.fares[0].passengerFares[0].serviceCharges[k].amount;
                                                finalamount += amount;

                                            }
                                            //TempData["fareTotalsum"] = fareTotalsum;

                                            decimal taxamount = finalamount;
                                            fareIndividual.taxamount = taxamount;
                                            fareIndividual.faretotal = fareAmount;
                                            fareIndividual.discountamount = discountamount;
                                            fareIndividual.passengertype = passengertype;
                                            fareIndividual.fareKey = fareAvailabilityKey;
                                            fareIndividual.procuctclass = procuctclass;
                                            fareIndividualsList.Add(fareIndividual);

                                        }

                                        var expandoconverter = new ExpandoObjectConverter();
                                        dynamic objR = JsonConvert.DeserializeObject<ExpandoObject>(destination.ToString(), expandoconverter);
                                        string jsonresultR = JsonConvert.SerializeObject(objR);
                                        _SimpleAvailibilityaAddResponceobjR = JsonConvert.DeserializeObject<SimpleAvailibilityaAddResponce>(jsonresultR);
                                        _SimpleAvailibilityaAddResponceobjR.designator = Designatorobj;
                                        _SimpleAvailibilityaAddResponceobjR.segments = Segmentobjlist;

                                        _SimpleAvailibilityaAddResponceobjR.arrivalTerminal = arrivalTerminal;
                                        _SimpleAvailibilityaAddResponceobjR.departureTerminal = departureTerminal;
                                        _SimpleAvailibilityaAddResponceobjR.bookingdate = bookingdate;
                                        _SimpleAvailibilityaAddResponceobjR.fareTotalsum = fareTotalsum;
                                        _SimpleAvailibilityaAddResponceobjR.journeyKey = journeyKey;
                                        _SimpleAvailibilityaAddResponceobjR.uniqueId = i;
                                        _SimpleAvailibilityaAddResponceobjR.faresIndividual = fareIndividualsList;
                                        _SimpleAvailibilityaAddResponceobjR.uniqueId = uniqueidx;
                                        _SimpleAvailibilityaAddResponceobjR.Airline = Airlines.Airasia;
                                        if (_SimpleAvailibilityaAddResponceobjR.fareTotalsum <= 0 || stopsR >= 2)
                                            continue;
                                        uniqueidx++;
                                        SimpleAvailibilityaAddResponcelistR.Add(_SimpleAvailibilityaAddResponceobjR);
                                    }
                                }
                            }


                        }
                        Sessionmanager.LogonResponse _logonResponseobjR = null;
                        if (flightclass != "B")
                        {
                            //To do
                            // Akasa Code Start Round **************
                            #region Akasa
                            // Login Token Genrate

                            airlineLogin loginAkasaR = new airlineLogin();

                            result1s = response.Content.ReadAsStringAsync().Result;
                            JsonObject = JsonConvert.DeserializeObject<List<_credentials>>(result1s);
                            //Akasa Round login

                            var _CredentialsAkashaR = new _credentials();
                            await _credentialService.PopulateCredentialsAsync(response, _CredentialsAkashaR, 2);

                            loginAkasaR.credentials = _CredentialsAkashaR;
                            //TempData["AkasaLogin"] = login.credentials.Image;
                            //AkasaTokan AkasaTokan = new AkasaTokan();
                            AirasiaTokan AkasaTokanR = new AirasiaTokan();
                            var AkasaloginRequestdataR = JsonConvert.SerializeObject(loginAkasaR, Formatting.Indented);
                            logs.WriteLogsR(AkasaloginRequestdataR, "1-Tokan_Request", "AkasaRT");
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            HttpResponseMessage responceAkasaR = await client.PostAsJsonAsync(AppUrlConstant.AkasaTokan, loginAkasaR);
                            if (responceAkasaR.IsSuccessStatusCode)
                            {
                                var results = responceAkasaR.Content.ReadAsStringAsync().Result;
                                logs.WriteLogsR(results, "1-Token_Responce", "AkasaRT");
                                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject("") + "\n Response: " + results, "Login", "AkasaRT");
                                var JsonObj = JsonConvert.DeserializeObject<dynamic>(results);
                                AkasaTokanR.token = JsonObj.data.token;
                                AkasaTokanR.idleTimeoutInMinutes = JsonObj.data.idleTimeoutInMinutes;

                            }
                            //AkasaTokan Return Flight**********
                            HttpContext.Session.SetString("AkasaTokanR", JsonConvert.SerializeObject(AkasaTokanR.token));
                            //till here
                            //GetAvailibility****************
                            _SimpleAvailabilityobjR = new DomainLayer.Model.SimpleAvailabilityRequestModel();
                            _SimpleAvailabilityobjR.origin = _GetfligthModel.destination;
                            _SimpleAvailabilityobjR.destination = _GetfligthModel.origin;
                            _SimpleAvailabilityobjR.searchDestinationMacs = true;
                            _SimpleAvailabilityobjR.searchOriginMacs = true;
                            _SimpleAvailabilityobjR.beginDate = _GetfligthModel.endDate;
                            _SimpleAvailabilityobj.endDate = _GetfligthModel.endDate; //"2023-12-20";//_GetfligthModel.endDate;
                            _SimpleAvailabilityobjR.getAllDetails = true;
                            _SimpleAvailabilityobjR.taxesAndFees = "TaxesAndFees";
                            _codesR = new Codessimple();
                            _typeslistR = new List<Typesimple>();
                            if (_GetfligthModel.passengercount != null)
                            {
                                AdtTypeR = _GetfligthModel.passengercount.adulttype;
                                AdtCountR = _GetfligthModel.passengercount.adultcount;
                                chdtypeR = _GetfligthModel.passengercount.childtype;
                                chdcountR = _GetfligthModel.passengercount.childcount;
                                infanttypeR = _GetfligthModel.passengercount.infanttype;
                                infantcountR = _GetfligthModel.passengercount.infantcount;
                            }
                            else
                            {
                                AdtTypeR = _GetfligthModel.adulttype;
                                AdtCountR = _GetfligthModel.adultcount;
                                chdtypeR = _GetfligthModel.childtype;
                                chdcountR = _GetfligthModel.childcount;
                                infanttypeR = _GetfligthModel.infanttype;
                                infantcountR = _GetfligthModel.infantcount;
                            }
                            if (AdtTypeR == "ADT" && AdtCountR != 0)
                            {
                                Typesimple TypesR = new Typesimple();
                                TypesR.type = AdtType;
                                TypesR.count = AdtCount;

                                _typeslistR.Add(TypesR);
                            }
                            if (chdtypeR == "CHD" && chdcountR != 0)
                            {
                                Typesimple TypesR = new Typesimple();
                                TypesR.type = chdtype;
                                TypesR.count = chdcount;

                                _typeslistR.Add(TypesR);
                            }
                            if (infanttypeR == "INFT" && infantcountR != 0)
                            {
                                Typesimple TypesR = new Typesimple();
                                TypesR.type = infanttype;
                                TypesR.count = infantcount;

                                _typeslistR.Add(TypesR);
                            }


                            _PassengerssimpleR = new Passengerssimple();
                            _PassengerssimpleR.types = _typeslist;
                            _SimpleAvailabilityobjR.passengers = _Passengerssimple;

                            //_codes.currencyCode = "INR";
                            _SimpleAvailabilityobjR.codes = _codes;
                            //_SimpleAvailabilityobjR.sourceOrganization = "";
                            ///_SimpleAvailabilityobjR.currentSourceOrganization = "";
                            //_SimpleAvailabilityobjR.promotionCode = "";
                            sortOptionsR = new string[1];
                            sortOptionsR[0] = "NoSort";
                            string[] fareTypesR = new string[3];
                            fareTypesR[0] = "R";
                            fareTypesR[1] = "V";
                            fareTypesR[2] = "S";
                            string[] productClassesR = new string[3];
                            productClassesR[0] = "EC";
                            productClassesR[1] = "AV";
                            productClassesR[2] = "SP";
                            FiltersR = new Filters();
                            //FiltersR.exclusionType = "Default";
                            //FiltersR.loyalty = "MonetaryOnly";
                            // FiltersR.includeAllotments = true;
                            //FiltersR.connectionType = "Both";
                            FiltersR.compressionType = "1";
                            FiltersR.groupByDate = false;
                            FiltersR.carrierCode = "QP";
                            FiltersR.type = "ALL";
                            FiltersR.sortOptions = sortOptionsR;
                            FiltersR.maxConnections = 4;
                            FiltersR.fareTypes = fareTypesR;
                            FiltersR.productClasses = productClassesR;
                            _SimpleAvailabilityobjR.filters = FiltersR;
                            //_SimpleAvailabilityobjR.taxesAndFees = "Taxes";
                            //_SimpleAvailabilityobjR.ssrCollectionsMode = "Leg";
                            _SimpleAvailabilityobjR.numberOfFaresPerJourney = 4;
                            jsonR = JsonConvert.SerializeObject(_SimpleAvailabilityobjR, Formatting.Indented);
                            logs.WriteLogsR(jsonR, "2-SimpleAvailability_Req", "AkasaRT");
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AkasaTokanR.token);
                            responceR = await client.PostAsJsonAsync(AppUrlConstant.AkasasearchsimpleR, _SimpleAvailabilityobjR);
                            if (responceR.IsSuccessStatusCode)
                            {
                                var resultsR = responceR.Content.ReadAsStringAsync().Result;
                                logs.WriteLogsR(resultsR, "2-SimpleAvailability_Res", "AkasaRT");
                                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_SimpleAvailabilityobjR) + "\n Response: " + resultsR, "GetAvailibility", "AkasaRT");
                                var JsonObjR = JsonConvert.DeserializeObject<dynamic>(resultsR);


                                var oriDes = _GetfligthModel.destination + "|" + _GetfligthModel.origin;
                                TempData["originR"] = _SimpleAvailabilityobjR.origin;
                                TempData["destinationR"] = _SimpleAvailabilityobjR.destination;
                                // data binding 
                                if (JsonObjR.data.results != null && ((JArray)JsonObjR.data.results).Count > 0)
                                {
                                    if (((JArray)JsonObjR.data.results[0].trips).Count > 0)
                                    {
                                        var finddate = JsonObjR.data.results[0].trips[0].date;
                                        var bookingdate = finddate.ToString("dddd, dd MMMM yyyy");
                                        int count = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes].Count;
                                        TempData["countr"] = count;
                                        for (int i = 0; i < JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes].Count; i++)
                                        {
                                            var journeyR = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i];
                                            var stopsR = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].stops;

                                            string journeyKey = journeyR.journeyKey;
                                            //journeyKey1 = ((Newtonsoft.Json.Linq.JValue)journeyKey).Value.ToString();
                                            var destination = journeyR;
                                            Designator Designatorobj = new Designator();
                                            Designatorobj.origin = journeyR.designator.destination;
                                            Designatorobj.departure = journeyR.designator.departure;
                                            Designatorobj.arrival = journeyR.designator.arrival;


                                            string queryorigin = journeyR.designator.origin;
                                            origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                                            Designatorobj.origin = origin;
                                            string querydestination = journeyR.designator.destination;
                                            destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                            Designatorobj.destination = destination1;

                                            var segmentscount = journeyR.segments.Count;
                                            List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();

                                            for (int l = 0; l < segmentscount; l++)
                                            {
                                                DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                                Designator SegmentDesignatorobj = new Designator();
                                                var jousegR = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l];
                                                SegmentDesignatorobj.origin = jousegR.designator.origin;
                                                SegmentDesignatorobj.destination = jousegR.designator.destination;
                                                SegmentDesignatorobj.departure = jousegR.designator.departure;
                                                SegmentDesignatorobj.arrival = jousegR.designator.arrival;
                                                Segmentobj.designator = SegmentDesignatorobj;
                                                Identifier Identifier = new Identifier();
                                                Identifier.identifier = jousegR.identifier.identifier;
                                                Identifier.carrierCode = jousegR.identifier.carrierCode;
                                                Segmentobj.identifier = Identifier;

                                                int legscount = jousegR.legs.Count;
                                                List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();
                                                for (int m = 0; m < legscount; m++)
                                                {
                                                    DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                                    Designator legdesignatorobj = new Designator();
                                                    var Jousegleg = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[l].legs[m];
                                                    legdesignatorobj.origin = Jousegleg.designator.origin;
                                                    legdesignatorobj.destination = Jousegleg.designator.destination;
                                                    legdesignatorobj.departure = Jousegleg.designator.departure;
                                                    legdesignatorobj.arrival = Jousegleg.designator.arrival;
                                                    Legobj.designator = legdesignatorobj;
                                                    Legobj.legKey = Jousegleg.legKey;
                                                    Legobj.flightReference = Jousegleg.flightReference;
                                                    Leglist.Add(Legobj);
                                                    DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                                    LegInfo.arrivalTerminal = Jousegleg.legInfo.arrivalTerminal;
                                                    LegInfo.departureTerminal = Jousegleg.legInfo.departureTerminal;
                                                    LegInfo.arrivalTime = Jousegleg.legInfo.arrivalTime;
                                                    LegInfo.departureTime = Jousegleg.legInfo.departureTime;
                                                    Legobj.legInfo = LegInfo;

                                                }
                                                //  Leglist.Add(Legobj);
                                                Segmentobj.legs = Leglist;
                                                Segmentobjlist.Add(Segmentobj);

                                            }

                                            var terminal = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].segments[0].legs[0];
                                            var arrivalTerminal = terminal.legInfo.arrivalTerminal;
                                            var departureTerminal = terminal.legInfo.departureTerminal;
                                            int AkasaFareCount = journeyR.fares.Count;
                                            _SimpleAvailibilityaAddResponceobjR = new SimpleAvailibilityaAddResponce();
                                            if (AkasaFareCount > 0)
                                            {
                                                List<FareIndividual> AkasafareIndividualsList = new List<FareIndividual>();

                                                for (int j = 0; j < AkasaFareCount; j++)
                                                {
                                                    //x.data.results[0].trips[0].journeysAvailableByMarket["DEL|BLR"][0].fares[0].fareAvailabilityKey


                                                    FareIndividual AkasafareIndividual = new FareIndividual();


                                                    string fareAvailabilityKey = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].fares[j].fareAvailabilityKey;
                                                    //fareIndividual.faretotal = JsonObj.data.faresAvailable[fareAvailabilityKey].faretotal;
                                                    Total Akasatotal = new Total();
                                                    var bookingamount = JsonObjR.data.faresAvailable[fareAvailabilityKey].totals.fareTotal;

                                                    string fareAvailabilityKeyhead = JsonObjR.data.results[0].trips[0].journeysAvailableByMarket[oriDes][i].fares[0].fareAvailabilityKey;
                                                    var fare = JsonObjR.data.faresAvailable[fareAvailabilityKey];
                                                    var fareAvilableCount = fare.fares.Count;
                                                    var isGoverning = fare.fares[0].isGoverning;

                                                    var procuctclass = fare.fares[0].productClass;

                                                    var passengertype = fare.fares[0].passengerFares[0].passengerType;

                                                    int passengercount = adultcount + chdcount;
                                                    var perpersontotal = fare.totals.fareTotal;
                                                    var fareAmount = perpersontotal / passengercount;
                                                    var perpersontotalclasswise = fare.totals.fareTotal;
                                                    if (j == 0)
                                                    {
                                                        fareTotalsum = perpersontotalclasswise / passengercount;
                                                    }

                                                    //END
                                                    decimal discountamount = fare.fares[0].passengerFares[0].discountedFare;

                                                    int servicecharge = fare.fares[0].passengerFares[0].serviceCharges.Count;
                                                    decimal finalamount = 0;
                                                    for (int k = 1; k < servicecharge; k++)
                                                    {

                                                        decimal amount = fare.fares[0].passengerFares[0].serviceCharges[k].amount;
                                                        finalamount += amount;

                                                    }
                                                    //TempData["fareTotalsum"] = fareTotalsum;

                                                    decimal taxamount = finalamount;
                                                    AkasafareIndividual.taxamount = taxamount;
                                                    AkasafareIndividual.faretotal = fareAmount;
                                                    AkasafareIndividual.discountamount = discountamount;
                                                    AkasafareIndividual.passengertype = passengertype;
                                                    AkasafareIndividual.fareKey = fareAvailabilityKey;
                                                    AkasafareIndividual.procuctclass = procuctclass;
                                                    AkasafareIndividualsList.Add(AkasafareIndividual);

                                                }

                                                var expandoconverter = new ExpandoObjectConverter();
                                                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(destination.ToString(), expandoconverter);
                                                string jsonresult = JsonConvert.SerializeObject(obj);
                                                //to do
                                                _SimpleAvailibilityaAddResponceobjR = JsonConvert.DeserializeObject<SimpleAvailibilityaAddResponce>(jsonresult);

                                                _SimpleAvailibilityaAddResponceobjR.designator = Designatorobj;
                                                _SimpleAvailibilityaAddResponceobjR.segments = Segmentobjlist;
                                                _SimpleAvailibilityaAddResponceobjR.arrivalTerminal = arrivalTerminal;
                                                _SimpleAvailibilityaAddResponceobjR.departureTerminal = departureTerminal;
                                                _SimpleAvailibilityaAddResponceobjR.bookingdate = bookingdate;
                                                _SimpleAvailibilityaAddResponceobjR.fareTotalsum = fareTotalsum;
                                                _SimpleAvailibilityaAddResponceobjR.journeyKey = journeyKey;
                                                _SimpleAvailibilityaAddResponceobjR.faresIndividual = AkasafareIndividualsList;
                                                _SimpleAvailibilityaAddResponceobjR.uniqueId = i;
                                                _SimpleAvailibilityaAddResponceobjR.Airline = Airlines.AkasaAir;
                                                _SimpleAvailibilityaAddResponceobjR.uniqueId = uniqueidx;
                                                if (_SimpleAvailibilityaAddResponceobjR.fareTotalsum <= 0 || stopsR >=2)
                                                    continue;
                                                uniqueidx++;
                                                //SimpleAvailibilityaAddResponcelist.Add(_SimpleAvailibilityaAddResponceobj);
                                                SimpleAvailibilityaAddResponcelistR.Add(_SimpleAvailibilityaAddResponceobjR);
                                            }
                                        }
                                    }

                                }
                            }

                            #endregion
                            // Akasa Code End Round******************

                            //Roundtripcode for SpiceJet
                            #region spicejet
                            List<SimpleAvailibilityaAddResponce> SpiceJetAvailibilityaAddResponcelistR = new List<SimpleAvailibilityaAddResponce>();
                            //Logon 
                            #region Logon
                            Spicejet._login objSpicejetR_ = new Spicejet._login();
                            _SpicejetlogonResponseobjR = await objSpicejetR_.Login(SameAirlineRT, "SpicejetRT", AppUrlConstant.BaseURL);
                            //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_logonRequestobjR) + "\n Response: " + JsonConvert.SerializeObject(_logonResponseobjR), "Logon", "SpiceJetRT");


                            #endregion
                            //GetAvailability
                            #region GetAvailability
                            if (_SpicejetlogonResponseobjR != null)
                            {

                                GetAvailabilityVer2Response _getAvailabilityReturnRS = null;
                                GetAvailabilityRequest _getAvailabilityReturnRQ = null;
                                _getAvailabilityReturnRQ = new GetAvailabilityRequest();

                                SpicejetBookingManager_.GetAvailabilityVer2Response _getAvailabilityVer2ReturnResponse = null;
                                //SpicejetBookingManager_.GetAvailabilityVer2Response _getAvailabilityRS = null;
                                OnionArchitectureAPI.Services.Spicejet._GetAvailability objspicejetgetAvail_ = new OnionArchitectureAPI.Services.Spicejet._GetAvailability(httpContextAccessorInstance);
                                _getAvailabilityVer2ReturnResponse = await objspicejetgetAvail_.GetTripAvailability(_GetfligthModel, _SpicejetlogonResponseobjR, TotalCount, adultcount, childcount, infantcount, flightclass, SameAirlineRT, "SpicejetRT");

                                //_getAvailabilityReturnRQ.Signature = _SpicejetlogonResponseobjR.Signature;
                                //_getAvailabilityReturnRQ.ContractVersion = _SpicejetlogonResponseobjR.ContractVersion;


                                ////_GetfligthModel.origin = "BOM";
                                ////_GetfligthModel.destination = "IXJ";
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest = new TripAvailabilityRequest();
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests = new AvailabilityRequest[1];
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0] = new AvailabilityRequest();

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].DepartureStation = _GetfligthModel.destination; //return_origin

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].ArrivalStation = _GetfligthModel.origin; //return_depart
                                //TempData["originR"] = _GetfligthModel.origin;
                                //TempData["destinationR"] = _GetfligthModel.destination;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDateSpecified = true;
                                ////_getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDate = Convert.ToDateTime("2024-01-18");
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].BeginDate = Convert.ToDateTime(_GetfligthModel.endDate);

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDateSpecified = true;
                                ////_getAvailabilityRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDate = Convert.ToDateTime("2024-01-18");
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].EndDate = Convert.ToDateTime(_GetfligthModel.endDate);

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].FlightTypeSpecified = true;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].FlightType = FlightType.All;

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCountSpecified = true;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxCount = Convert.ToInt16(TotalCount); //Total Travell Count

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].DowSpecified = true;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].Dow = DOW.Daily;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].CurrencyCode = "INR";

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].AvailabilityFilter = default;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].AvailabilityFilterSpecified = true;


                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxPriceTypes = new PaxPriceType[0];
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].PaxPriceTypes = getPaxdetails(adultcount, childcount, infantcount); //Pax Count 1 always Default Set.


                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].CarrierCode = "SG";

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareClassControlSpecified = true;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareClassControl = FareClassControl.CompressByProductClass;

                                //string[] faretypesreturn = { "R", "MX", "SF" };
                                //// string[] faretypes = {"R"};
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].FareTypes = faretypesreturn;

                                //string[] productclassesreturn = new string[1];
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].ProductClasses = productclassesreturn;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].MaximumConnectingFlights = 20;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].MaximumConnectingFlightsSpecified = true;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].LoyaltyFilterSpecified = true;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].LoyaltyFilter = LoyaltyFilter.MonetaryOnly;

                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].IncludeTaxesAndFees = true;
                                //_getAvailabilityReturnRQ.TripAvailabilityRequest.AvailabilityRequests[0].IncludeTaxesAndFeesSpecified = true;

                                //HttpContext.Session.SetString("SpicejetReturnSignature", JsonConvert.SerializeObject(_logonResponseobjR.Signature));
                                //HttpContext.Session.SetString("SpicejetAvailibilityRequest", JsonConvert.SerializeObject(_getAvailabilityReturnRQ));

                                //GetAvailabilityVer2Response _getAvailabilityVer2ReturnResponse = await objSpiceJetR.GetAvailabilityVer2Async(_getAvailabilityReturnRQ);

                                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(_getAvailabilityReturnRQ) + "\n\n Response: " + JsonConvert.SerializeObject(_getAvailabilityVer2ReturnResponse), "GetAvailability", "SpiceJetRT");


                                count2 = 0;
                                if (_getAvailabilityVer2ReturnResponse != null && _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0].Length > 0)
                                {

                                    count2 = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys.Length;
                                }
                                for (int i = 0; i < count2; i++)
                                {
                                    string _journeysellkey = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;

                                    _SimpleAvailibilityaAddResponceobjR = new SimpleAvailibilityaAddResponce();
                                    string journeyKey = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                                    Designator Designatorobj = new Designator();
                                    Designatorobj.origin = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                                    Designatorobj.destination = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;

                                    string journeykey = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey.ToString();
                                    //string departureTime = Regex.Match(journeykey, @Designatorobj.origin + @"[\s\S]*?\s(?<STD>[\s\S]*?)~").Groups["STD"].Value.Trim();
                                    //string arrivalTime = Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?\s(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                                    string departureTime = Regex.Match(journeykey, @Designatorobj.origin + @"[\s\S]*?~(?<STD>[\s\S]*?)~").Groups["STD"].Value.Trim();
                                    string arrivalTime = Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();

                                    //Designatorobj.departure = Convert.ToDateTime(departureTime);


                                    //Designatorobj.arrival = Convert.ToDateTime(arrivalTime);

                                    Designatorobj.departure = DateTime.ParseExact(departureTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(departureTime);
                                    Designatorobj.arrival = DateTime.ParseExact(arrivalTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(arrivalTime);

                                    string queryorigin = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                                    origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                                    string querydestination = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;
                                    destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                    Designatorobj.destination = destination1;
                                    Designatorobj.origin = origin;

                                    var segmentscount = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment.Length;
                                    List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();
                                    List<FareIndividual> fareIndividualsList = new List<FareIndividual>();
                                    List<FareIndividual> fareIndividualsconnectedList = new List<FareIndividual>();
                                    decimal discountamount = 0M;// JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].discountedFare;
                                    decimal finalamount = 0;
                                    decimal taxamount = 0M;
                                    for (int l = 0; l < segmentscount; l++)
                                    {
                                        DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                        Designator SegmentDesignatorobj = new Designator();
                                        SegmentDesignatorobj.origin = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].DepartureStation;
                                        SegmentDesignatorobj.destination = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].ArrivalStation; ;
                                        SegmentDesignatorobj.departure = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STD;
                                        SegmentDesignatorobj.arrival = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STA;
                                        Segmentobj.designator = SegmentDesignatorobj;
                                        Identifier Identifier = new Identifier();
                                        Identifier.identifier = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].FlightDesignator.FlightNumber; ;
                                        Identifier.carrierCode = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].FlightDesignator.CarrierCode;
                                        Segmentobj.identifier = Identifier;

                                        int legscount = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs.Length;
                                        List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();

                                        for (int m = 0; m < legscount; m++)
                                        {
                                            DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                            Designator legdesignatorobj = new Designator();
                                            legdesignatorobj.origin = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].DepartureStation; ;
                                            legdesignatorobj.destination = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].ArrivalStation;
                                            legdesignatorobj.departure = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].STD;
                                            legdesignatorobj.arrival = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].STA;
                                            Legobj.designator = legdesignatorobj;
                                            Leglist.Add(Legobj);

                                            DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                            LegInfo.arrivalTerminal = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.ArrivalTerminal;
                                            LegInfo.departureTerminal = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.DepartureTerminal;
                                            LegInfo.arrivalTime = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.PaxSTA;
                                            LegInfo.departureTime = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.PaxSTD;
                                            var arrivalTerminal = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.ArrivalTerminal;
                                            var departureTerminal = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.DepartureTerminal;

                                            Legobj.legInfo = LegInfo;


                                            _SimpleAvailibilityaAddResponceobjR.arrivalTerminal = arrivalTerminal;
                                            _SimpleAvailibilityaAddResponceobjR.departureTerminal = departureTerminal;

                                        }
                                        Segmentobj.legs = Leglist;
                                        Segmentobjlist.Add(Segmentobj);
                                        FareIndividual fareIndividual = new FareIndividual();
                                        for (int k2 = 0; k2 < _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares.Length; k2++)
                                        {

                                            string fareindex = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares[k2].FareIndex.ToString();

                                            #region fare
                                            int FareCount = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares.Length;

                                            if (FareCount > 0)
                                            {
                                                try
                                                {
                                                    for (int j = 0; j < FareCount; j++)
                                                    {
                                                        if (fareindex == j.ToString())
                                                        {

                                                            fareIndividual = new FareIndividual();
                                                            string _fareSellkey = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                            string fareAvailabilityKey = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                            string fareAvailabilityKeyhead = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                            var procuctclass = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].ProductClass;
                                                            var passengertype = "";
                                                            decimal fareAmount = 0.0M;
                                                            int servicecharge = 0;
                                                            servicecharge = 0;
                                                            if (_getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].PaxFares.Length > 0)
                                                            {
                                                                passengertype = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].PaxType;
                                                                fareAmount = Math.Round(_getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[0].Amount);
                                                                fareTotalsum = Math.Round(_getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[0].Amount);
                                                                servicecharge = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges.Length;

                                                            }
                                                            else
                                                            {
                                                                continue;
                                                            }

                                                            finalamount = 0;
                                                            taxamount = 0M;
                                                            //for (int k = 1; k < servicecharge; k++) // one way
                                                            for (int k = 0; k < servicecharge; k++)
                                                            {
                                                                if (k > 0)
                                                                {
                                                                    taxamount = _getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[k].Amount;
                                                                    finalamount += taxamount;
                                                                }

                                                            }
                                                            taxamount = finalamount;
                                                            fareIndividual.taxamount = taxamount;
                                                            fareIndividual.faretotal = fareAmount + taxamount;
                                                            fareIndividual.discountamount = discountamount;
                                                            fareIndividual.passengertype = passengertype;
                                                            fareIndividual.fareKey = fareAvailabilityKey;
                                                            fareIndividual.procuctclass = procuctclass;

                                                            if (l > 0)
                                                            {
                                                                fareIndividualsconnectedList.Add(fareIndividual);
                                                            }
                                                            else
                                                            {
                                                                fareIndividualsList.Add(fareIndividual);

                                                            }
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                }
                                            }
                                        }
                                    }
                                    //fareIndividualsconnectedList = new List<FareIndividual>();
                                    if (segmentscount > 1)
                                    {
                                        for (int i1 = 0; i1 < fareIndividualsList.Count; i1++)
                                        {
                                            for (int i2 = 0; i2 < fareIndividualsconnectedList.Count; i2++)
                                            {
                                                if (fareIndividualsconnectedList[i2].procuctclass.Equals(fareIndividualsList[i1].procuctclass) && i2 == i1)
                                                {
                                                    fareIndividualsList[i1].fareKey += "^" + fareIndividualsconnectedList[i2].fareKey;
                                                    fareIndividualsList[i1].faretotal += fareIndividualsconnectedList[i2].faretotal;
                                                }
                                                else
                                                    continue;
                                            }
                                        }
                                        #endregion
                                        fareIndividualsList.RemoveAll(x => !x.fareKey.Contains("^"));
                                    }
                                    fareIndividualsconnectedList = fareIndividualsList;
                                    int StopCounter = 0;
                                    if (Segmentobjlist.Count == 1)
                                    {
                                        if (Segmentobjlist[0].legs.Count >= 1)
                                            StopCounter = Segmentobjlist[0].legs.Count;
                                    }
                                    else
                                        StopCounter = Segmentobjlist.Count;


                                    fareTotalsum = 0;
                                    //todo Viewprice
                                    decimal[] ViewPriceNew = new decimal[fareIndividualsconnectedList.Count];
                                    for (int d = 0; d < fareIndividualsconnectedList.Count; d++)
                                    {
                                        ViewPriceNew[d] = fareIndividualsconnectedList[d].faretotal;

                                    }
                                    Array.Sort(ViewPriceNew);
                                    if (ViewPriceNew.Length > 0 && ViewPriceNew[0] > 0)
                                    {
                                        fareTotalsum = ViewPriceNew[0];
                                    }
                                    _SimpleAvailibilityaAddResponceobjR.stops = StopCounter - 1;
                                    _SimpleAvailibilityaAddResponceobjR.designator = Designatorobj;
                                    _SimpleAvailibilityaAddResponceobjR.segments = Segmentobjlist;
                                    DateTime currentDate = DateTime.Now;
                                    var bookingdate = currentDate; //"2023-12-10T00:00:00";
                                    _SimpleAvailibilityaAddResponceobjR.bookingdate = Convert.ToDateTime(_getAvailabilityVer2ReturnResponse.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureDate).ToString("dddd, dd MMM yyyy");// Convert.ToDateTime(bookingdate).ToString("dddd, dd MMM yyyy");
                                    _SimpleAvailibilityaAddResponceobjR.fareTotalsum = Math.Round(fareTotalsum, 0);

                                    _SimpleAvailibilityaAddResponceobjR.journeyKey = journeyKey;
                                    _SimpleAvailibilityaAddResponceobjR.faresIndividual = fareIndividualsconnectedList;// fareIndividualsList;
                                    _SimpleAvailibilityaAddResponceobjR.uniqueId = uniqueidx;
                                    _SimpleAvailibilityaAddResponceobjR.Airline = Airlines.Spicejet;
                                    if (_SimpleAvailibilityaAddResponceobjR.fareTotalsum <= 0)
                                        continue;
                                    uniqueidx++;
                                    SpiceJetAvailibilityaAddResponcelistR.Add(_SimpleAvailibilityaAddResponceobjR);
                                    SimpleAvailibilityaAddResponcelistR.Add(_SimpleAvailibilityaAddResponceobjR);
                                }


                                string str1Return = JsonConvert.SerializeObject(_getAvailabilityVer2ReturnResponse);
                            }
                            #endregion

                            #endregion
                        }

                        #region Indigo
                        List<SimpleAvailibilityaAddResponce> IndigoAvailibilityaAddResponcelistR = new List<SimpleAvailibilityaAddResponce>();
                        //Logon 
                        #region Logon
                        obj_ = new _login();
                        IndigoSessionmanager_.LogonResponse _IndigologonResponseobjR = await obj_.Login(SameAirlineRT, "IndigoRT", AppUrlConstant.BaseURL);
                        #endregion
                        //.GetAvailability
                        #region GetAvailability

                        httpContextAccessorInstance = new HttpContextAccessor();
                        objgetAvail_ = new OnionArchitectureAPI.Services.Indigo._GetAvailability(httpContextAccessorInstance);
                        string _origin = _GetfligthModel.origin;
                        string _destination = _GetfligthModel.destination;
                        _GetfligthModel.destination = _origin;
                        _GetfligthModel.origin = _destination;
                        _GetfligthModel.beginDate = _GetfligthModel.endDate;
                        TempData["originR"] = _GetfligthModel.origin;
                        TempData["destinationR"] = _GetfligthModel.destination;
                        IndigoBookingManager_.GetAvailabilityVer2Response _IndigoAvailabilityResponseobjR = await objgetAvail_.GetTripAvailability(_GetfligthModel, _IndigologonResponseobjR, TotalCount, adultcount, childcount, infantcount, flightclass, SameAirlineRT);
                        count2 = 0;
                        if (_IndigoAvailabilityResponseobjR != null && _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0].Length > 0)
                        {
                            count2 = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys.Length;
                        }
                        for (int i = 0; i < count2; i++)
                        {
                            string _journeysellkey = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                            _SimpleAvailibilityaAddResponceobjR = new SimpleAvailibilityaAddResponce();
                            string journeyKey = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                            Designator Designatorobj = new Designator();
                            Designatorobj.origin = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                            Designatorobj.destination = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;
                            string journeykey = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey.ToString();
                            string departureTime = Regex.Match(journeykey, @Designatorobj.origin + @"[\s\S]*?~(?<STD>[\s\S]*?)~").Groups["STD"].Value.Trim();
                            string arrivalTime = Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                            Designatorobj.departure = DateTime.ParseExact(departureTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(departureTime);
                            Designatorobj.arrival = DateTime.ParseExact(arrivalTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(arrivalTime);

                            string queryorigin = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                            origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                            string querydestination = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;
                            destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                            Designatorobj.destination = destination1;
                            Designatorobj.origin = origin;
                            //Designatorobj.departure = Convert.ToDateTime(departureTime);
                            //Designatorobj.arrival = Convert.ToDateTime(arrivalTime);


                            var segmentscount = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment.Length;
                            List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();
                            List<FareIndividual> fareIndividualsList = new List<FareIndividual>();
                            List<FareIndividual> fareIndividualsconnectedList = new List<FareIndividual>();
                            decimal taxamount = 0M;
                            decimal discountamount = 0M;// JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].discountedFare;
                            decimal finalamount = 0;
                            for (int l = 0; l < segmentscount; l++)
                            {
                                DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                Designator SegmentDesignatorobj = new Designator();
                                SegmentDesignatorobj.origin = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].DepartureStation;
                                SegmentDesignatorobj.destination = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].ArrivalStation; ;
                                SegmentDesignatorobj.departure = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STD;
                                SegmentDesignatorobj.arrival = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].STA;
                                Segmentobj.designator = SegmentDesignatorobj;
                                Identifier Identifier = new Identifier();
                                Identifier.identifier = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].FlightDesignator.FlightNumber; ;
                                Identifier.carrierCode = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].FlightDesignator.CarrierCode;
                                Segmentobj.identifier = Identifier;
                                //if(Identifier.identifier == "5039")
                                //{

                                //}
                                int legscount = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs.Length;
                                List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();
                                for (int m = 0; m < legscount; m++)
                                {
                                    DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                    Designator legdesignatorobj = new Designator();
                                    legdesignatorobj.origin = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].DepartureStation; ;
                                    legdesignatorobj.destination = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].ArrivalStation;
                                    legdesignatorobj.departure = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].STD;
                                    legdesignatorobj.arrival = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].STA;
                                    Legobj.designator = legdesignatorobj;
                                    Leglist.Add(Legobj);
                                    DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                    LegInfo.arrivalTerminal = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.ArrivalTerminal;
                                    LegInfo.departureTerminal = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.DepartureTerminal;
                                    LegInfo.arrivalTime = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.PaxSTA;
                                    LegInfo.departureTime = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.PaxSTD;
                                    var arrivalTerminal = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.ArrivalTerminal;
                                    var departureTerminal = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].Legs[m].LegInfo.DepartureTerminal;
                                    Legobj.legInfo = LegInfo;
                                    _SimpleAvailibilityaAddResponceobjR.arrivalTerminal = arrivalTerminal;
                                    _SimpleAvailibilityaAddResponceobjR.departureTerminal = departureTerminal;
                                }
                                Segmentobj.legs = Leglist;
                                Segmentobjlist.Add(Segmentobj);
                                FareIndividual fareIndividual = new FareIndividual();
                                for (int k2 = 0; k2 < _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares.Length; k2++)
                                {
                                    string fareindex = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].AvailableSegment[l].AvailableFares[k2].FareIndex.ToString();
                                    #region fare
                                    int FareCount = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares.Length;
                                    if (FareCount > 0)
                                    {
                                        try
                                        {
                                            for (int j = 0; j < FareCount; j++)
                                            {
                                                if (fareindex == j.ToString())
                                                {
                                                    fareIndividual = new FareIndividual();
                                                    string _fareSellkey = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    string fareAvailabilityKey = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    string fareAvailabilityKeyhead = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    var procuctclass = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].ProductClass;
                                                    var passengertype = "";
                                                    decimal fareAmount = 0.0M;
                                                    int servicecharge = 0;
                                                    servicecharge = 0;
                                                    if (_IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].PaxFares.Length > 0)
                                                    {
                                                        passengertype = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].PaxType;
                                                        fareAmount = Math.Round(_IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[0].Amount);
                                                        fareTotalsum = Math.Round(_IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[0].Amount);
                                                        servicecharge = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges.Length;
                                                    }
                                                    else
                                                    {
                                                        // for pick farekey in case of amount is zero or priceamount is null in connected flight in RoundTrip
                                                    }

                                                    finalamount = 0;
                                                    taxamount = 0M;
                                                    for (int k = 0; k < servicecharge; k++)
                                                    {
                                                        if (k > 0)
                                                        {
                                                            taxamount = _IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Fares[j].PaxFares[0].ServiceCharges[k].Amount;
                                                            finalamount += taxamount;
                                                        }
                                                    }
                                                    taxamount = finalamount;
                                                    fareIndividual.taxamount = taxamount;
                                                    fareIndividual.faretotal = fareAmount + taxamount;
                                                    fareIndividual.discountamount = discountamount;
                                                    fareIndividual.passengertype = passengertype;
                                                    fareIndividual.fareKey = fareAvailabilityKey;
                                                    fareIndividual.procuctclass = procuctclass;
                                                    if (l > 0)
                                                    {
                                                        fareIndividualsconnectedList.Add(fareIndividual);
                                                    }
                                                    else
                                                    {
                                                        fareIndividualsList.Add(fareIndividual);

                                                    }
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                    }
                                }
                            }
                            //fareIndividualsconnectedList = new List<FareIndividual>();
                            if (segmentscount > 1)
                            {
                                for (int i1 = 0; i1 < fareIndividualsList.Count; i1++)
                                {
                                    for (int i2 = 0; i2 < fareIndividualsconnectedList.Count; i2++)
                                    {
                                        if (fareIndividualsconnectedList[i2].procuctclass.Equals(fareIndividualsList[i1].procuctclass) && i2 == i1)
                                        {
                                            fareIndividualsList[i1].fareKey += "^" + fareIndividualsconnectedList[i2].fareKey;
                                            fareIndividualsList[i1].faretotal += fareIndividualsconnectedList[i2].faretotal;
                                        }
                                        else
                                            continue;
                                    }
                                }
                                #endregion
                                fareIndividualsList.RemoveAll(x => !x.fareKey.Contains("^"));
                            }
                            fareIndividualsconnectedList = fareIndividualsList;
                            //fareIndividualsconnectedList = fareIndividualsconnectedList.Where(d => d.fareKey.Contains('^')).ToList();
                            int StopCounter = 0;
                            if (Segmentobjlist.Count == 1)
                            {
                                if (Segmentobjlist[0].legs.Count >= 1)
                                    StopCounter = Segmentobjlist[0].legs.Count;
                            }
                            else
                                StopCounter = Segmentobjlist.Count;

                            var duplicates = fareIndividualsconnectedList.GroupBy(x => x.procuctclass).Where(g => g.Count() > 1).SelectMany(g => g).ToHashSet();

                            // Remove all items that are duplicates
                            fareIndividualsconnectedList = fareIndividualsconnectedList.Where(item => !duplicates.Contains(item)).ToList();


                            fareTotalsum = 0;
                            //todo Viewprice
                            decimal[] ViewPriceNew = new decimal[fareIndividualsconnectedList.Count];
                            for (int d = 0; d < fareIndividualsconnectedList.Count; d++)
                            {
                                ViewPriceNew[d] = fareIndividualsconnectedList[d].faretotal;

                            }
                            Array.Sort(ViewPriceNew);
                            if (ViewPriceNew.Length > 0 && ViewPriceNew[0] > 0)
                            {
                                fareTotalsum = ViewPriceNew[0];
                            }
                            _SimpleAvailibilityaAddResponceobjR.stops = StopCounter - 1;
                            _SimpleAvailibilityaAddResponceobjR.designator = Designatorobj;
                            _SimpleAvailibilityaAddResponceobjR.segments = Segmentobjlist;
                            DateTime currentDate = DateTime.Now;
                            var bookingdate = currentDate; //"2023-12-10T00:00:00";
                            //_SimpleAvailibilityaAddResponceobjR.bookingdate = Convert.ToDateTime(_IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureDate).ToString("dddd, dd MMM yyyy");
                            //_SimpleAvailibilityaAddResponceobjR.bookingdate = Convert.ToDateTime(bookingdate).ToString("dddd, dd MMM yyyy");
                            if (_IndigoAvailabilityResponseobjR == null)
                            {
                                _SimpleAvailibilityaAddResponceobjR.bookingdate = bookingdate.ToString(); ;
                            }
                            else
                            {
                                _SimpleAvailibilityaAddResponceobjR.bookingdate = Convert.ToDateTime(_IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureDate).ToString("dddd, dd MMM yyyy");
                            }
                            _SimpleAvailibilityaAddResponceobjR.fareTotalsum = Math.Round(fareTotalsum, 0);
                            _SimpleAvailibilityaAddResponceobjR.journeyKey = journeyKey;
                            _SimpleAvailibilityaAddResponceobjR.faresIndividual = fareIndividualsconnectedList;// fareIndividualsList;
                            _SimpleAvailibilityaAddResponceobjR.uniqueId = uniqueidx;
                            _SimpleAvailibilityaAddResponceobjR.Airline = Airlines.Indigo;
                            if (_SimpleAvailibilityaAddResponceobjR.fareTotalsum <= 0)
                                continue;
                            uniqueidx++;
                            //SpiceJetAvailibilityaAddResponcelistR.Add(_SimpleAvailibilityaAddResponceobjR);
                            SimpleAvailibilityaAddResponcelistR.Add(_SimpleAvailibilityaAddResponceobjR);
                        }
                        if (_IndigoAvailabilityResponseobjR != null)
                        {
                            str2Return = JsonConvert.SerializeObject(_IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response);
                        }
                        #endregion
                        #endregion

                        #region GDS
                        // string _testURL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/AirService";
                        _testURL = AppUrlConstant.GDSURL;
                        _targetBranch = string.Empty;
                        _userName = string.Empty;
                        _password = string.Empty;
                        res = string.Empty;
                        sbReq = null;

                        //GDS login

                        _CredentialsGDS = new _credentials();
                        if (response.IsSuccessStatusCode)
                        {
                            var results = response.Content.ReadAsStringAsync().Result;
                            JsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);
                            await _credentialService.PopulateCredentialsAsync(response, _CredentialsGDS, 5);
                            //if (JsonObject[4].FlightCode == 5)
                            //{
                            //    _CredentialsGDS.username = JsonObject[4].username;
                            //    _CredentialsGDS.password = JsonObject[4].password;
                            //    _CredentialsGDS.domain = JsonObject[4].domain;
                            //    // _CredentialsGDS.Status = JsonObject[4].Status;
                            //}


                        }
                        //_targetBranch = "P7027135";
                        //_userName = "Universal API/uAPI5098257106-beb65aec";
                        //_password = "Q!f5-d7A3D";
                        sbReq = new StringBuilder();
                        Guid newGuidR = Guid.NewGuid();
                        httpContextAccessorInstance = new HttpContextAccessor();
                        _objAvail = null;
                        _objAvail = new TravelPort(httpContextAccessorInstance);
                        res = _objAvail.GetAvailabilty(_testURL, sbReq, _objAvail, _GetfligthModel, newGuidR.ToString(), _CredentialsGDS.domain, _CredentialsGDS.username, _CredentialsGDS.password, flightclass, SameAirlineRT, "");
                        TempData["originR"] = _GetfligthModel.origin;
                        TempData["destinationR"] = _GetfligthModel.destination;
                        _objP = new TravelPortParsing();
                        getAvailRes = new List<GDSResModel.Segment>();
                        if (res != null && !res.Contains("Bad Request") && !res.Contains("Internal Server Error"))
                        {
                            getAvailRes = _objP.ParseLowFareSearchRsp2(res, "OneWay", Convert.ToDateTime(_GetfligthModel.beginDate));
                        }

                        // to do
                        count2 = 0;
                        if (getAvailRes != null && getAvailRes.Count > 0)
                        {
                            count2 = getAvailRes.Count;
                        }
                        for (int i1 = 0; i1 < count2; i1++)
                        {
                            for (int k1 = 0; k1 < getAvailRes[i1].Bonds.Count; k1++)
                            {
                                string _journeysellkey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                                _SimpleAvailibilityaAddResponceobjR = new SimpleAvailibilityaAddResponce();
                                string journeyKey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey;
                                Designator Designatorobj = new Designator();

                                if (getAvailRes[i1].Bonds[k1].BoundType.ToLower() == "outbound")
                                {
                                    try
                                    {
                                        List<SimpleAvailibilityaAddResponce> matchingItineraries1 = SimpleAvailibilityaAddResponcelistR.Where(it => it.Identifier == getAvailRes[i1].Bonds[k1].FlightNumber).ToList();
                                        //if (matchingItineraries1.Count>0 && matchingItineraries1[0].Identifier=="811")
                                        //{

                                        //}
                                        //if (matchingItineraries1.Count == 1)
                                        //continue;
                                    }
                                    catch (Exception ex)
                                    {

                                    }


                                    Designatorobj.origin = getAvailRes[i1].Bonds[k1].Legs[0].Origin;//_IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureStation;
                                    Designatorobj.destination = getAvailRes[i1].Bonds[k1].Legs[0].Destination; // _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].ArrivalStation;
                                    string journeykey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Schedules[0][0].AvailableJourneys[i].JourneySellKey.ToString();
                                    string departureTime = getAvailRes[i1].Bonds[k1].Legs[0].DepartureTime;// Regex.Match(journeykey, @Designatorobj.origin + @"[\s\S]*?~(?<STD>[\s\S]*?)~").Groups["STD"].Value.Trim()
                                    string arrivalTime = getAvailRes[i1].Bonds[k1].Legs[0].ArrivalTime;// ; Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                                    Designatorobj.departure = DateTimeOffset.Parse(getAvailRes[i1].Bonds[k1].Legs[0].DepartureTime).DateTime; //Convert.ToDateTime(getAvailRes[i1].Bonds[0].Legs[0].DepartureTime); // DateTime.ParseExact(departureTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(departureTime);
                                    if (getAvailRes[i1].Bonds[k1].Legs.Count == 3)
                                    {
                                        Designatorobj.arrival = DateTimeOffset.Parse(getAvailRes[i1].Bonds[k1].Legs[2].ArrivalTime).DateTime; // Convert.ToDateTime(getAvailRes[i1].Bonds[0].Legs[2].ArrivalTime);
                                    }
                                    else if (getAvailRes[i1].Bonds[k1].Legs.Count == 2)
                                    {
                                        Designatorobj.arrival = DateTimeOffset.Parse(getAvailRes[i1].Bonds[k1].Legs[1].ArrivalTime).DateTime; //Convert.ToDateTime(getAvailRes[i1].Bonds[0].Legs[1].ArrivalTime);
                                    }
                                    else
                                    {
                                        Designatorobj.arrival = DateTimeOffset.Parse(getAvailRes[i1].Bonds[k1].Legs[0].ArrivalTime).DateTime;//Convert.ToDateTime(getAvailRes[i1].Bonds[0].Legs[0].ArrivalTime);
                                    }
                                    //Designatorobj.arrival = Convert.ToDateTime(getAvailRes[i1].Bonds[k1].Legs[0].ArrivalTime); // DateTime.ParseExact(arrivalTime, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture); //Convert.ToDateTime(arrivalTime);
                                    Designatorobj.Arrival = "";// Regex.Match(journeykey, @Designatorobj.destination + @"[\s\S]*?~(?<STA>[\s\S]*?)~").Groups["STA"].Value.Trim();
                                                               //DateTime IarrivalDateTime = DateTime.ParseExact(Designatorobj.arrival, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
                                                               //Designatorobj.ArrivalDate = IarrivalDateTime.ToString("yyyy-MM-dd");
                                                               //Designatorobj.ArrivalTime = IarrivalDateTime.ToString("HH:mm:ss");
                                    TimeSpan TimeDifference = Designatorobj.arrival - Designatorobj.departure;
                                    TimeSpan timeSpan = TimeSpan.Parse(TimeDifference.ToString());
                                    if (timeSpan.Minutes == 0)
                                        formatTime = $"{(int)timeSpan.TotalHours} h";
                                    else
                                        formatTime = $"{(int)timeSpan.TotalHours} h {timeSpan.Minutes} m";
                                    Designatorobj.formatTime = timeSpan;
                                    //vivek
                                    //Designatorobj.SetformatTime = formatTime;
                                    string queryorigin = getAvailRes[i1].Bonds[k1].Legs[0].Origin;
                                    origin = Citynamelist.GetAllCityData().Where(x => x.citycode == queryorigin).SingleOrDefault().cityname;
                                    Designatorobj.origin = origin;
                                    string querydestination = string.Empty;
                                    if (getAvailRes[i1].Bonds[k1].Legs.Count == 3)
                                    {
                                        querydestination = getAvailRes[i1].Bonds[k1].Legs[2].Destination;
                                        destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                        Designatorobj.destination = destination1;
                                    }
                                    else
                                    {
                                        if (getAvailRes[i1].Bonds[k1].Legs.Count > 1)
                                        {
                                            querydestination = getAvailRes[i1].Bonds[k1].Legs[1].Destination;
                                            destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                            Designatorobj.destination = destination1;

                                        }
                                        else
                                        {
                                            querydestination = getAvailRes[i1].Bonds[k1].Legs[0].Destination;
                                            destination1 = Citynamelist.GetAllCityData().Where(x => x.citycode == querydestination).SingleOrDefault().cityname;
                                            Designatorobj.destination = destination1;
                                        }
                                    }

                                    var segmentscount = getAvailRes[i1].Bonds[k1].Legs.Count;
                                    List<DomainLayer.Model.Segment> Segmentobjlist = new List<DomainLayer.Model.Segment>();
                                    List<FareIndividual> fareIndividualsList = new List<FareIndividual>();
                                    List<FareIndividual> fareIndividualsconnectedList = new List<FareIndividual>();
                                    decimal discountamount = 0M;
                                    decimal finalamount = 0;
                                    decimal taxamount = 0M;
                                    int IndoStopcounter = 0;
                                    for (int l = 0; l < segmentscount; l++)
                                    {
                                        DomainLayer.Model.Segment Segmentobj = new DomainLayer.Model.Segment();
                                        Designator SegmentDesignatorobj = new Designator();
                                        SegmentDesignatorobj.origin = getAvailRes[i1].Bonds[k1].Legs[l].Origin;
                                        SegmentDesignatorobj.destination = getAvailRes[i1].Bonds[k1].Legs[l].Destination;

                                        SegmentDesignatorobj.departure = Convert.ToDateTime(getAvailRes[i1].Bonds[k1].Legs[l].DepartureTime);
                                        SegmentDesignatorobj.arrival = Convert.ToDateTime(getAvailRes[i1].Bonds[k1].Legs[l].ArrivalTime);

                                        SegmentDesignatorobj._DepartureDate = getAvailRes[i1].Bonds[k1].Legs[l].DepartureTime;
                                        SegmentDesignatorobj._AvailabilitySource = getAvailRes[i1].Bonds[k1].Legs[l]._AvailabilitySource;
                                        SegmentDesignatorobj._AvailabilityDisplayType = getAvailRes[i1].Bonds[k1].Legs[l]._AvailabilityDisplayType;
                                        SegmentDesignatorobj._FlightTime = getAvailRes[i1].Bonds[k1].Legs[l].Duration;
                                        SegmentDesignatorobj._Equipment = getAvailRes[i1].Bonds[k1].Legs[l]._Equipment;
                                        SegmentDesignatorobj._Distance = getAvailRes[i1].Bonds[k1].Legs[l]._Distance;
                                        SegmentDesignatorobj._ArrivalDate = getAvailRes[i1].Bonds[k1].Legs[l]._ArrivalDate;
                                        SegmentDesignatorobj._Group = getAvailRes[i1].Bonds[k1].Legs[l].Group;
                                        SegmentDesignatorobj._ProviderCode = getAvailRes[i1].Bonds[k1].Legs[l].ProviderCode;
                                        SegmentDesignatorobj._ClassOfService = getAvailRes[i1].Bonds[k1].Legs[l].FareClassOfService;


                                        Segmentobj.designator = SegmentDesignatorobj;
                                        Identifier Identifier = new Identifier();
                                        Identifier.identifier = getAvailRes[i1].Bonds[k1].Legs[l].FlightNumber;
                                        //if (Identifier.identifier == "817")
                                        //{
                                        //var t = SimpleAvailibilityaAddResponcelist[0].segments[0].identifier.identifier.ToString();
                                        //}
                                        Identifier.carrierCode = getAvailRes[i1].Bonds[k1].Legs[l].CarrierCode;
                                        //to do && it.segments[0].identifier.carrierCode == getAvailRes[i1].Bonds[k1].Legs[0].CarrierCode
                                        Segmentobj.identifier = Identifier;
                                        int legscount = 1;// getAvailRes[i1].Bonds[k1].Legs.Count;
                                        List<DomainLayer.Model.Leg> Leglist = new List<DomainLayer.Model.Leg>();
                                        for (int m = 0; m < legscount; m++)
                                        {
                                            DomainLayer.Model.Leg Legobj = new DomainLayer.Model.Leg();
                                            Designator legdesignatorobj = new Designator();
                                            legdesignatorobj.origin = getAvailRes[i1].Bonds[k1].Legs[l].Origin;
                                            legdesignatorobj.destination = getAvailRes[i1].Bonds[k1].Legs[l].Destination;
                                            legdesignatorobj.departure = Convert.ToDateTime(getAvailRes[i1].Bonds[k1].Legs[l].DepartureTime);
                                            legdesignatorobj.arrival = Convert.ToDateTime(getAvailRes[i1].Bonds[k1].Legs[l].ArrivalTime);
                                            Legobj.designator = legdesignatorobj;

                                            DomainLayer.Model.LegInfo LegInfo = new DomainLayer.Model.LegInfo();
                                            LegInfo.arrivalTerminal = getAvailRes[i1].Bonds[k1].Legs[l].ArrivalTerminal;
                                            LegInfo.departureTerminal = getAvailRes[i1].Bonds[k1].Legs[l].DepartureTerminal;
                                            LegInfo.arrivalTime = Convert.ToDateTime(getAvailRes[i1].Bonds[k1].Legs[l].ArrivalTime);
                                            LegInfo.departureTime = Convert.ToDateTime(getAvailRes[i1].Bonds[k1].Legs[l].DepartureTime);
                                            var arrivalTerminal = getAvailRes[i1].Bonds[k1].Legs[l].ArrivalTerminal;
                                            var departureTerminal = getAvailRes[i1].Bonds[k1].Legs[l].DepartureTerminal;
                                            Legobj.legInfo = LegInfo;
                                            Leglist.Add(Legobj);
                                            _SimpleAvailibilityaAddResponceobjR.arrivalTerminal = arrivalTerminal;
                                            _SimpleAvailibilityaAddResponceobjR.departureTerminal = departureTerminal;
                                        }

                                        Segmentobj.legs = Leglist;
                                        Segmentobjlist.Add(Segmentobj);
                                        decimal fareAmount = 0.0M;
                                        fareAmount = Math.Round(getAvailRes[i1].Fare.PaxFares[0].BasicFare, 0);
                                        FareIndividual fareIndividual = new FareIndividual();

                                        List<GDSResModel.Segment> matchingItineraries = getAvailRes.Where(it => it.Segmentid == getAvailRes[i1].Segmentid).ToList();
                                        string s = JsonConvert.SerializeObject(matchingItineraries);
                                        if (matchingItineraries.Count > 0)
                                        {
                                            try
                                            {
                                                for (int j = 0; j < matchingItineraries.Count; j++)
                                                {

                                                    fareIndividual = new FareIndividual();
                                                    string _fareSellkey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    string fareAvailabilityKey = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    string fareAvailabilityKeyhead = "";// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].FareSellKey;
                                                    var procuctclass = matchingItineraries[j].Bonds[k1].Legs[l].Branddesc;// _IndigoAvailabilityResponseobj.GetTripAvailabilityVer2Response.Fares[j].ProductClass;
                                                    fareAvailabilityKey = matchingItineraries[j].Bonds[k1].Legs[l]._FareBasisCodeforAirpriceHit;
                                                    var passengertype = "";
                                                    fareAmount = 0.0M;
                                                    int servicecharge = 0;
                                                    servicecharge = 0;
                                                    passengertype = matchingItineraries[j].Fare.PaxFares[0].PaxType.ToString();
                                                    fareAmount = Math.Round(matchingItineraries[j].Fare.PaxFares[0].BasicFare, 0);
                                                    fareTotalsum = Math.Round(matchingItineraries[j].Fare.PaxFares[0].BasicFare, 0);
                                                    taxamount = Math.Round(matchingItineraries[j].Fare.PaxFares[0].TotalTax, 0);

                                                    discountamount = 0M;// JsonObj.data.faresAvailable[fareAvailabilityKey].fares[0].passengerFares[0].discountedFare;
                                                    fareIndividual.taxamount = taxamount;
                                                    fareIndividual.faretotal = fareAmount + taxamount;
                                                    fareIndividual.discountamount = discountamount;
                                                    fareIndividual.passengertype = passengertype;
                                                    fareIndividual.fareKey = fareAvailabilityKey;
                                                    fareIndividual.procuctclass = procuctclass;

                                                    if (l > 0)
                                                    {
                                                        fareIndividualsconnectedList.Add(fareIndividual);
                                                    }
                                                    else
                                                    {
                                                        fareIndividualsList.Add(fareIndividual);

                                                    }

                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }

                                        if (string.IsNullOrEmpty(_SimpleAvailibilityaAddResponceobjR.Identifier))
                                        {
                                            _SimpleAvailibilityaAddResponceobjR.Identifier = getAvailRes[i1].Bonds[k1].Legs[l].FlightNumber;
                                        }
                                        else
                                        {
                                            _SimpleAvailibilityaAddResponceobjR.Identifier += "@" + getAvailRes[i1].Bonds[k1].Legs[l].FlightNumber;
                                        }

                                        if (string.IsNullOrEmpty(_SimpleAvailibilityaAddResponceobjR.SegmentidRightdata))
                                        {
                                            _SimpleAvailibilityaAddResponceobjR.SegmentidRightdata = getAvailRes[i1].Bonds[k1].Legs[l].AircraftCode;
                                            _SimpleAvailibilityaAddResponceobjR.FareBasisRightdata = getAvailRes[i1].Bonds[k1].Legs[l]._FareBasisCodeforAirpriceHit;
                                        }
                                        else
                                        {
                                            _SimpleAvailibilityaAddResponceobjR.SegmentidRightdata += "@" + getAvailRes[i1].Bonds[k1].Legs[l].AircraftCode;
                                            _SimpleAvailibilityaAddResponceobjR.FareBasisRightdata += "@" + getAvailRes[i1].Bonds[k1].Legs[l]._FareBasisCodeforAirpriceHit;
                                        }
                                    }
                                    IndoStopcounter += segmentscount;
                                    if (segmentscount > 1)
                                    {
                                        for (int ia = 0; ia < fareIndividualsList.Count; ia++)
                                        {
                                            for (int i2 = 0; i2 < fareIndividualsconnectedList.Count; i2++)
                                            {
                                                if (fareIndividualsconnectedList[i2].procuctclass != null && fareIndividualsconnectedList[i2].procuctclass.Equals(fareIndividualsList[ia].procuctclass) && i2 == ia)
                                                {
                                                    fareIndividualsList[ia].fareKey += "^" + fareIndividualsconnectedList[i2].fareKey;
                                                    fareIndividualsList[ia].faretotal = fareIndividualsconnectedList[i2].faretotal;
                                                }
                                                else
                                                    continue;
                                            }
                                        }
                                        fareIndividualsList.RemoveAll(x => !x.fareKey.Contains("^"));
                                    }

                                    fareIndividualsconnectedList = fareIndividualsList;

                                    fareTotalsum = 0;
                                    //todo Viewprice
                                    decimal[] ViewPriceNew = new decimal[fareIndividualsconnectedList.Count];
                                    for (int d = 0; d < fareIndividualsconnectedList.Count; d++)
                                    {
                                        ViewPriceNew[d] = fareIndividualsconnectedList[d].faretotal;

                                    }
                                    Array.Sort(ViewPriceNew);
                                    if (ViewPriceNew.Length > 0 && ViewPriceNew[0] > 0)
                                    {
                                        fareTotalsum = ViewPriceNew[0];
                                    }

                                    _SimpleAvailibilityaAddResponceobjR.Segmentiddata = getAvailRes[i1].Segmentid;
                                    _SimpleAvailibilityaAddResponceobjR.stops = IndoStopcounter - 1;
                                    _SimpleAvailibilityaAddResponceobjR.designator = Designatorobj;
                                    _SimpleAvailibilityaAddResponceobjR.segments = Segmentobjlist;
                                    DateTime currentDate = DateTime.Now;
                                    var bookingdate1 = currentDate; //"2023-12-10T00:00:00";
                                    _SimpleAvailibilityaAddResponceobjR.bookingdate = Convert.ToDateTime(Segmentobjlist[0].designator._DepartureDate).ToString("dddd, dd MMM yyyy");// Convert.ToDateTime(bookingdate1).ToString("dddd, dd MMM yyyy");
                                    //if (_IndigoAvailabilityResponseobj == null)
                                    //{
                                    // _SimpleAvailibilityaAddResponceobjR.bookingdate = bookingdate1.ToString(); ;
                                    //}
                                    //else
                                    //{
                                    //_SimpleAvailibilityaAddResponceobjR.bookingdate = Convert.ToDateTime(_IndigoAvailabilityResponseobjR.GetTripAvailabilityVer2Response.Schedules[0][0].DepartureDate).ToString("dddd, dd MMM yyyy");
                                    //}
                                    _SimpleAvailibilityaAddResponceobjR.fareTotalsum = Math.Round(fareTotalsum, 0);
                                    _SimpleAvailibilityaAddResponceobjR.journeyKey = journeyKey;
                                    _SimpleAvailibilityaAddResponceobjR.faresIndividual = fareIndividualsconnectedList;// fareIndividualsList;
                                    _SimpleAvailibilityaAddResponceobjR.uniqueId = uniqueidx;
                                    if (_SimpleAvailibilityaAddResponceobjR.segments[0].identifier.carrierCode.Equals("UK"))
                                        _SimpleAvailibilityaAddResponceobjR.Airline = Airlines.Vistara;
                                    else if (_SimpleAvailibilityaAddResponceobjR.segments[0].identifier.carrierCode.Equals("AI"))
                                        _SimpleAvailibilityaAddResponceobjR.Airline = Airlines.AirIndia;
                                    else if (_SimpleAvailibilityaAddResponceobjR.segments[0].identifier.carrierCode.Equals("H1"))
                                        _SimpleAvailibilityaAddResponceobjR.Airline = Airlines.Hehnair;
                                    if (_SimpleAvailibilityaAddResponceobjR.fareTotalsum <= 0)
                                        continue;
                                    uniqueidx++;
                                    //SpiceJetAvailibilityaAddResponcelistR.Add(_SimpleAvailibilityaAddResponceobjR);
                                    SimpleAvailibilityaAddResponcelistR.Add(_SimpleAvailibilityaAddResponceobjR);
                                }
                            }
                        }
                        #endregion


                        //end
                        HttpContext.Session.SetString("LeftReturnViewFlightView", JsonConvert.SerializeObject(SimpleAvailibilityaAddResponcelist));
                        //Home page request
                        HttpContext.Session.SetString("PassengerModel", JsonConvert.SerializeObject(_SimpleAvailabilityobj));
                        //TempData["PassengerModel"] = JsonConvert.SerializeObject(_SimpleAvailabilityobj);

                        HttpContext.Session.SetString("RightReturnFlightView", JsonConvert.SerializeObject(SimpleAvailibilityaAddResponcelistR));
                        //AirAsia
                        //TempData["Mymodel"] = JsonConvert.SerializeObject(SimpleAvailibilityaAddResponcelist);
                        //TempData["PassengerModel"] = JsonConvert.SerializeObject(_SimpleAvailabilityobj);


                        //RoundTrip
                        //TempData["MymodelR"] = JsonConvert.SerializeObject(SimpleAvailibilityaAddResponcelistR);
                        //TempData["PassengerModelR"] = JsonConvert.SerializeObject(_SimpleAvailabilityobjR);
                        HttpContext.Session.SetString("PassengerModelR", JsonConvert.SerializeObject(_SimpleAvailabilityobj));
                        if (_SpicejetlogonResponseobjR != null)
                        {
                            HttpContext.Session.SetString("SpicejetSignatureR", JsonConvert.SerializeObject(_SpicejetlogonResponseobjR.Signature));
                        }

                        //Indigo

                        //RoundTrip
                        HttpContext.Session.SetString("IndigoSignatureR", JsonConvert.SerializeObject(_IndigologonResponseobjR.Signature));
                        HttpContext.Session.SetString("GDSTraceidR", JsonConvert.SerializeObject(newGuidR.ToString()));
                        ////SpiceJet
                        //TempData["SpiceJetmodel"] = JsonConvert.SerializeObject(SpiceJetAvailibilityaAddResponcelist);
                        //TempData["SpiceJetPassengerModel"] = JsonConvert.SerializeObject(_getAvailabilityRQ);
                        HttpContext.Session.SetString("SpiceJetPassengerModelR", JsonConvert.SerializeObject(_getAvailabilityRQ));
                        return RedirectToAction("RTFlightView", "RoundTrip", new { TripType = SameAirlineRT, Origin = orgincity, OriginCode = orgincode, Destination = destinationCity, DestinationCode = destinationcode, BeginDate = _GetfligthModel.beginDate, EndDate = _GetfligthModel.endDate, AdultCount = _GetfligthModel.passengercount != null ? _GetfligthModel.passengercount.adultcount : _GetfligthModel.adultcount, ChildCount = _GetfligthModel.passengercount != null ? _GetfligthModel.passengercount.childcount : _GetfligthModel.childcount, InfantCount = _GetfligthModel.passengercount != null ? _GetfligthModel.passengercount.infantcount : _GetfligthModel.infantcount });
                    }
                    else
                    {

                        //Indigo

                        //OneWayTrip
                        //HttpContext.Session.SetString("IndigoSignature", JsonConvert.SerializeObject(_IndigologonResponseobj.Signature));
                        HttpContext.Session.SetString("OneWayFlightView", JsonConvert.SerializeObject(SimpleAvailibilityaAddResponcelist));
                        HttpContext.Session.SetString("OneWayPassengerModel", JsonConvert.SerializeObject(_SimpleAvailabilityobj));

                        // TempData["Mymodel"] = JsonConvert.SerializeObject(SimpleAvailibilityaAddResponcelist);
                        //TempData["PassengerModel"] = JsonConvert.SerializeObject(_SimpleAvailabilityobj);
                        //HttpContext.Session.SetString("Mymodel", JsonConvert.SerializeObject(SimpleAvailibilityaAddResponcelist));
                        HttpContext.Session.SetString("PassengerModel", JsonConvert.SerializeObject(_SimpleAvailabilityobj));

                        ////SpiceJet
                        //TempData["SpiceJetmodel"] = JsonConvert.SerializeObject(SpiceJetAvailibilityaAddResponcelist);
                        //TempData["SpiceJetPassengerModel"] = JsonConvert.SerializeObject(_getAvailabilityRQ);
                        HttpContext.Session.SetString("SpiceJetPassengerModel", JsonConvert.SerializeObject(_getAvailabilityRQ));

                        //Redis
                        searlizetext = JsonConvert.SerializeObject(SimpleAvailibilityaAddResponcelist);
                        //logs.WriteLogs("\n Response: " + searlizetext, "simpleavailabiliotydata", "GDSOneWay");

                        // encodedlist = Encoding.UTF8.GetBytes(searlizetext);
                        var option = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(60)).SetAbsoluteExpiration(TimeSpan.FromSeconds(300));
                        //await _distributedCache.SetAsync(KeyName, encodedlist, option);
                        //await _distributedCache.SetStringAsync(KeyName, encodedlist, option);
                        await _distributedCache.SetStringAsync(KeyName, searlizetext, option);

                        //return RedirectToAction("FlightView", "ResultFlightView");
                        //return RedirectToAction("FlightView", "ResultFlightView", new { key1 = "value1", key2 = "value2" });
                        //AdultCount = _GetfligthModel.passengercount.adultcount
                        //AdultCount = _GetfligthModel.passengercount != null ? _GetfligthModel.passengercount.adultcount : _GetfligthModel.adultcount,




                        return RedirectToAction("FlightView", "ResultFlightView", new { Guid = SearchGuid, TripType = SameAirlineRT, Origin = orgincity, OriginCode = orgincode, Destination = destinationCity, DestinationCode = destinationcode, BeginDate = _GetfligthModel.beginDate, AdultCount = _GetfligthModel.passengercount != null ? _GetfligthModel.passengercount.adultcount : _GetfligthModel.adultcount, ChildCount = _GetfligthModel.passengercount != null ? _GetfligthModel.passengercount.childcount : _GetfligthModel.childcount, InfantCount = _GetfligthModel.passengercount != null ? _GetfligthModel.passengercount.infantcount : _GetfligthModel.infantcount, FlightClass = flightclass });

                    }

                }

            }

        }
        public IActionResult PassengeDetails(Passengers passengers)
        {
            Passengers passengers1 = new Passengers();
            List<_Types> types = new List<_Types>();
            passengers1.types = passengers.types;
            return View();
        }

        PaxPriceType[] getPaxdetails(int adult_, int child_, int infant_)
        {
            PaxPriceType[] paxPriceTypes = null;
            try
            {
                //int tcount = adult_ + child_ + infant_;
                int i = 0;
                if (adult_ > 0) i++;
                if (child_ > 0) i++;
                if (infant_ > 0) i++;

                paxPriceTypes = new PaxPriceType[i];
                int j = 0;
                if (adult_ > 0)
                {
                    paxPriceTypes[j] = new PaxPriceType();
                    paxPriceTypes[j].PaxType = "ADT";
                    paxPriceTypes[j].PaxCountSpecified = true;
                    paxPriceTypes[j].PaxCount = Convert.ToInt16(adult_);
                    //paxPriceTypes[j].PaxCount = Convert.ToInt16(0);
                    j++;
                }

                if (child_ > 0)
                {
                    paxPriceTypes[j] = new PaxPriceType();
                    paxPriceTypes[j].PaxType = "CHD";
                    paxPriceTypes[j].PaxCountSpecified = true;
                    paxPriceTypes[j].PaxCount = Convert.ToInt16(child_);
                    //paxPriceTypes[j].PaxCount = Convert.ToInt16(0);
                    j++;
                }

                if (infant_ > 0)
                {
                    paxPriceTypes[j] = new PaxPriceType();
                    paxPriceTypes[j].PaxType = "INFT";
                    paxPriceTypes[j].PaxCountSpecified = true;
                    paxPriceTypes[j].PaxCount = Convert.ToInt16(infant_);
                    //paxPriceTypes[j].PaxCount = Convert.ToInt16(0);
                    j++;
                }
            }
            catch (Exception e)
            {
            }

            return paxPriceTypes;
        }


        public PointOfSale GetPointOfSale()
        {
            PointOfSale SourcePOS = null;
            try
            {
                SourcePOS = new PointOfSale();
                SourcePOS.State = Bookingmanager_.MessageState.New;
                SourcePOS.OrganizationCode = "APITESTID";
                SourcePOS.AgentCode = "AG";
                SourcePOS.LocationCode = "";
                SourcePOS.DomainCode = "WWW";
            }
            catch (Exception e)
            {
                string exp = e.Message;
                exp = null;
            }
            return SourcePOS;
        }


        [HttpGet]
        public IActionResult GetFilteredCities(string query)
        {

            var allCities = Citynamelist.GetAllCityData();

            var cities = allCities.Where(c => c.citycode.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .Concat(allCities.Where(c => c.cityname.StartsWith(query, StringComparison.OrdinalIgnoreCase)))
                .Concat(allCities.Where(c => c.airportname.StartsWith(query, StringComparison.OrdinalIgnoreCase)))
                //.Take(100)
                .ToList();



            var distinctCities = cities.GroupBy(c => new { c.citycode, c.cityname, c.airportname }).Select(g => g.First()).ToList();
            return Json(distinctCities);
            //return Json(cities);
        }

    }
}

