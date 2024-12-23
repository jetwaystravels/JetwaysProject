﻿using DomainLayer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using ServiceLayer.Service.Interface;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using Utility;
using ZXing.QrCode.Internal;
using static DomainLayer.Model.GDSResModel;
using static DomainLayer.Model.ReturnTicketBooking;

namespace OnionArchitectureAPI.Services.Travelport
{
    public class TravelPort : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TravelPort(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetSessionValue(string key, string value)
        {
            _httpContextAccessor.HttpContext.Session.SetString(key, value);
        }
        Logs logs = new Logs();
        //string _targetBranch = string.Empty;
        //string _userName = string.Empty;
        //string _password = string.Empty;
        //public TravelPort(string tragetBranch_, string userName_, string password_)
        //{
        //    _targetBranch = tragetBranch_;
        //    _userName = userName_;
        //    _password = password_;
        //}
        public string GetAvailabiltyRT(string _testURL, StringBuilder sbReq, TravelPort _objAvail, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, string flightclass, string _AirlineWay)
        {

            sbReq = new StringBuilder();
            sbReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sbReq.Append("<soap:Body>");
            sbReq.Append("<LowFareSearchReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" SolutionResult=\"true\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _targetBranch + "\" ReturnUpsellFare =\"true\">");
            sbReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            sbReq.Append("<SearchAirLeg>");
            sbReq.Append("<SearchOrigin>");
            sbReq.Append("<CityOrAirport xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"" + _GetfligthModel.origin + "\" PreferCity=\"true\" />");
            sbReq.Append("</SearchOrigin>");
            sbReq.Append("<SearchDestination>");
            sbReq.Append("<CityOrAirport xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"" + _GetfligthModel.destination + "\" PreferCity=\"true\" />");
            sbReq.Append("</SearchDestination>");
            sbReq.Append("<SearchDepTime PreferredTime=\"" + _GetfligthModel.beginDate + "\"/>");
            sbReq.Append("</SearchAirLeg>");

            sbReq.Append("<SearchAirLeg>");
            sbReq.Append("<SearchOrigin>");
            sbReq.Append("<CityOrAirport xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"" + _GetfligthModel.destination + "\" PreferCity=\"true\" />");
            sbReq.Append("</SearchOrigin>");
            sbReq.Append("<SearchDestination>");
            sbReq.Append("<CityOrAirport xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"" + _GetfligthModel.origin + "\" PreferCity=\"true\" />");
            sbReq.Append("</SearchDestination>");
            sbReq.Append("<SearchDepTime PreferredTime=\"" + _GetfligthModel.endDate + "\"/>");
            sbReq.Append("</SearchAirLeg>");

            sbReq.Append("<AirSearchModifiers>");
            sbReq.Append("<PreferredProviders>");
            sbReq.Append("<Provider xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"1G\" />");
            sbReq.Append("</PreferredProviders>");

            // Start for prohibited carrier
            sbReq.Append("<ProhibitedCarriers>");
            sbReq.Append("<Carrier Code='H1' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            sbReq.Append("</ProhibitedCarriers>");
            //End  for prohibited carrier

            // Business class
            if (flightclass == "B")
            {
                sbReq.Append("<PermittedCabins>");
                sbReq.Append("<CabinClass Type=\"PremiumEconomy\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                sbReq.Append("</PermittedCabins>");
            }

            //Permitted Carrier
            //sbReq.Append("<air:PermittedCarriers xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
            //sbReq.Append("<Carrier Code='9W' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='AI' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='UK' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("</air:PermittedCarriers>");

            sbReq.Append("</AirSearchModifiers>");
            int pax = 0;
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                    }
                }

                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"true\" Age=\"01\"/>");
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"10\"/>");
                    }
                }
            }
            else
            {
                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                    }
                }
                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"true\" Age=\"01\"/>");
                    }
                }
                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        pax++;
                        sbReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"10\"/>");
                    }
                }



            }
            sbReq.Append("<AirPricingModifiers FaresIndicator=\"AllFares\" ETicketability=\"Required\">");
            sbReq.Append("<AccountCodes>");
            sbReq.Append("<AccountCode xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"-\" />");
            sbReq.Append("</AccountCodes>");
            sbReq.Append("<FlightType TripleInterlineCon=\"false\" DoubleInterlineCon=\"false\" SingleInterlineCon=\"true\" TripleOnlineCon=\"false\" DoubleOnlineCon=\"false\" SingleOnlineCon=\"true\" StopDirects=\"true\" NonStopDirects=\"true\" />");
            sbReq.Append("</AirPricingModifiers>");
            sbReq.Append("</LowFareSearchReq></soap:Body></soap:Envelope>");

            string res = Methodshit.HttpPost(_testURL, sbReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + sbReq + "\n\n Response: " + res, "GetAvailability", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(sbReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAvailability", "GDSRT");
            }
            return res;
        }

        public string GetAvailabilty(string _testURL, StringBuilder sbReq, TravelPort _objAvail, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, string flightclass, string JourneyType, string _AirlineWay)
        {

            sbReq = new StringBuilder();
            sbReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sbReq.Append("<soap:Body>");
            sbReq.Append("<air:LowFareSearchReq xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" xmlns:air=\"http://www.travelport.com/schema/air_v52_0\"  AuthorizedBy=\"Travelport\" SolutionResult=\"true\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _targetBranch + "\" ReturnUpsellFare =\"true\">");
            sbReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            sbReq.Append("<air:SearchAirLeg>");
            sbReq.Append("<air:SearchOrigin>");
            sbReq.Append("<com:CityOrAirport Code=\"" + _GetfligthModel.origin + "\"/>");
            sbReq.Append("</air:SearchOrigin>");
            sbReq.Append("<air:SearchDestination>");
            sbReq.Append("<com:CityOrAirport Code=\"" + _GetfligthModel.destination + "\"/>");
            sbReq.Append("</air:SearchDestination>");

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                sbReq.Append("<air:SearchDepTime PreferredTime=\"" + _GetfligthModel.beginDate + "\"/>");
            }
            else
            {
                sbReq.Append("<air:SearchDepTime PreferredTime=\"" + _GetfligthModel.endDate + "\"/>");

            }
            sbReq.Append("</air:SearchAirLeg>");
            sbReq.Append("<air:AirSearchModifiers>");
            sbReq.Append("<air:PreferredProviders>");
            sbReq.Append("<com:Provider Code=\"1G\" />");
            sbReq.Append("</air:PreferredProviders>");

            // Start for prohibited carrier
            //sbReq.Append("<ProhibitedCarriers>");
            //sbReq.Append("<Carrier Code='H1' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("</ProhibitedCarriers>");
            //End  for prohibited carrier

            // Business class
            if (flightclass == "B")
            {
                sbReq.Append("<air:PermittedCabins>");
                sbReq.Append("<com:CabinClass Type=\"Business\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                sbReq.Append("</air:PermittedCabins>");
            }

            // Economy Premium class
            if (flightclass == "P")
            {
                sbReq.Append("<air:PermittedCabins>");
                sbReq.Append("<com:CabinClass Type=\"PremiumEconomy\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                sbReq.Append("</air:PermittedCabins>");
            }

            //Permitted Carrier
            //sbReq.Append("<air:PermittedCarriers xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
            //sbReq.Append("<Carrier Code='9W' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='AI' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("<Carrier Code='UK' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            //sbReq.Append("</air:PermittedCarriers>");

            sbReq.Append("</air:AirSearchModifiers>");
            int pax = 0;
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                        pax++;
                    }
                }

                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        pax++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {

                        sbReq.Append("<com:SearchPassenger  Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"11\"/>");
                        pax++;
                    }
                }
            }
            else
            {
                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {

                        sbReq.Append("<com:SearchPassenger  Code=\"ADT\" BookingTravelerRef=\"" + pax + "\" />");
                        pax++;
                    }
                }
                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger  Code=\"INF\" BookingTravelerRef=\"" + pax + "\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        pax++;
                    }
                }
                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        sbReq.Append("<com:SearchPassenger Code=\"CNN\" BookingTravelerRef=\"" + pax + "\" Age=\"11\"/>");
                        pax++;
                    }
                }



            }
            sbReq.Append("<air:AirPricingModifiers FaresIndicator=\"AllFares\" ETicketability=\"Required\">");
            //sbReq.Append("<AccountCodes>");
            //sbReq.Append("<AccountCode xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"-\" />");
            //sbReq.Append("</AccountCodes>");
            sbReq.Append("<FlightType TripleInterlineCon=\"false\" DoubleInterlineCon=\"false\" SingleInterlineCon=\"true\" TripleOnlineCon=\"false\" DoubleOnlineCon=\"false\" SingleOnlineCon=\"true\" StopDirects=\"true\" NonStopDirects=\"true\" />");
            sbReq.Append("</air:AirPricingModifiers>");
            sbReq.Append("</air:LowFareSearchReq></soap:Body></soap:Envelope>");





            //sbReq.Append("<air:LowFareSearchReq xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" xmlns:air=\"http://www.travelport.com/schema/air_v52_0\" AuthorizedBy=\"ENDFARE\" ");
            //sbReq.Append("SolutionResult=\"true\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _objAvail._targetBranch + "\">");
            //sbReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            //sbReq.Append("<air:SearchAirLeg>");
            //sbReq.Append("<air:SearchOrigin><com:CityOrAirport Code=\"" + _GetfligthModel.origin + "\"/></air:SearchOrigin>");
            //sbReq.Append("<air:SearchDestination><com:CityOrAirport Code=\"" + _GetfligthModel.destination + "\"/></air:SearchDestination>");
            //sbReq.Append("<air:SearchDepTime PreferredTime=\"" + _GetfligthModel.beginDate + "\"/>");
            //sbReq.Append("<air:AirLegModifiers><air:PreferredCabins><com:CabinClass Type=\"Economy\"/></air:PreferredCabins></air:AirLegModifiers>");
            //sbReq.Append("</air:SearchAirLeg><air:AirSearchModifiers OrderBy=\"DepartureTime\">");
            //sbReq.Append("<air:PreferredProviders><com:Provider Code=\"1G\"/></air:PreferredProviders>");

            ////sbReq.Append("<air:PermittedCarriers xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
            ////sbReq.Append("<Carrier Code='9W' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            ////sbReq.Append("<Carrier Code='AI' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            ////sbReq.Append("<Carrier Code='UK' xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
            ////sbReq.Append("</air:PermittedCarriers>");
            //sbReq.Append("</air:AirSearchModifiers>");

            //if (_GetfligthModel.passengercount != null)
            //{
            //    if (_GetfligthModel.passengercount.adultcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"ADT\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUA01\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.passengercount.infantcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"INF\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUB02\" PricePTCOnly=\"true\" Age=\"01\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.passengercount.childcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"CNN\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUC03\" Age=\"10\"/>");
            //        }
            //    }
            //}
            //else
            //{

            //    if (_GetfligthModel.adultcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.adultcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"ADT\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUA01\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.infantcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.infantcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"INF\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUB02\" PricePTCOnly=\"true\" Age=\"01\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.childcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.childcount; i++)
            //        {
            //            sbReq.Append("<com:SearchPassenger Code=\"CNN\" BookingTravelerRef=\"ilay2SzXTkSUYRO+0owUC03\" Age=\"10\"/>");
            //        }
            //    }
            //}
            //sbReq.Append("<air:AirPricingModifiers FaresIndicator=\"AllFares\" ETicketability=\"Yes\" CurrencyType=\"INR\">");
            //sbReq.Append("<air:FlightType RequireSingleCarrier=\"true\" MaxConnections=\"1\" MaxStops=\"1\" NonStopDirects=\"true\" StopDirects=\"true\" SingleOnlineCon=\"true \"/></air:AirPricingModifiers>");
            //sbReq.Append("</air:LowFareSearchReq></soap:Body></soap:Envelope>");

            string res = Methodshit.HttpPost(_testURL, sbReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs("URL: " + _testURL + "\n\nRequest: " + sbReq, "1-GetAvailabilityReq", "GDSOneWay", JourneyType);
                logs.WriteLogs(res, "1-GetAvailabilityRes", "GDSOneWay", JourneyType);
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(sbReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAvailability", "GDSRT");
                logs.WriteLogsR("URL: " + _testURL + "\n\nRequest: " + sbReq, "1-GetAvailabilityReq", "GDSRT");
                logs.WriteLogsR(res, "1-GetAvailabilityRes", "GDSRT");
            }
            return res;
        }

        public string AirPriceGetRoundTrip(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, string farebasisdataL, int p, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do
            string segmentIdDataL = string.Empty;
            if (p == 0)
            {
                segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            }
            else
            {
                segmentIdDataL = AirfaredataL.SegmentidRightdata;
            }
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }



            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                count++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"1\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;

                    FarebasisDataL = FarebasisDataL0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                    FarebasisDataL = FarebasisDataL1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                    FarebasisDataL = FarebasisDataL2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(fareRepriceReq.ToString(), "3-GetAirpriceReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetAirpriceRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Right", "GDSRT");
                }
            }
            return res;
        }

        public string AirPriceGet(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, string farebasisdataL, int p, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do
            string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }



            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                count++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"1\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;

                    FarebasisDataL = FarebasisDataL0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                    FarebasisDataL = FarebasisDataL1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                    FarebasisDataL = FarebasisDataL2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(fareRepriceReq.ToString(), "3-GetAirpriceReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "2-GetAirpriceRes", "GDSOneWay", "oneway");
            }
            else
            {
                //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                if (p == 0)
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Left", "GDSRT");
                }
                else
                {
                    logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                    logs.WriteLogsR(res, "2-GetAirpriceRes_Right", "GDSRT");
                }
            }
            return res;
        }
        //Same Airline RoundTrip 26-09-2024
        public string AirPriceGetRT(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, dynamic AirfaredataL, dynamic AirfaredataR, string _AirlineWay)
        {

            int count = 0;
            int countR = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;
            int legKeyCounterR = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do Left
            string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
            }


            //Right
            string segmentIdDataR = AirfaredataR.SegmentidRightdata;
            string[] segmentIdsR = segmentIdDataR.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string segmentIdAtIndexR0 = string.Empty;
            string segmentIdAtIndexR1 = string.Empty;
            string segmentIdAtIndexR2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsR.Length == 3)
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
                segmentIdAtIndexR2 = segmentIdsR[2];

            }
            else if (segmentIdsR.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
            }
            else
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
            }

            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                count++;
            }

            foreach (var segment in AirfaredataR.segments)
            {
                if (countR == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR0;
                }
                else if (countR == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                countR++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Yes\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"10\"/>");
                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"01\"/>");
                    }
                }
            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                    }
                }



                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"10\"/>");
                    }
                }
                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        paxCount++;
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"01\"/>");
                    }
                }




            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }

            if (segmentIdsR.Length == 3)
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
                segmentIdAtIndexR2 = segmentIdsR[2];

            }
            else if (segmentIdsR.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
            }
            else
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
            }
            foreach (var segment in AirfaredataR.segments)
            {
                if (legKeyCounterR == 0)
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR0;
                }
                else if (legKeyCounterR == 1)
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR1;
                }
                else
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounterR++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
            }
            return res;
        }

        public string AirPriceGetRT_V2(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, SimpleAvailibilityaAddResponce AirfaredataL, SimpleAvailibilityaAddResponce AirfaredataR, string farebasisdataL, string farebasisdataR, string _AirlineWay)
        {

            int count = 0;
            int countR = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;
            int legKeyCounterR = 0;

            fareRepriceReq = new StringBuilder();

            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            //fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\"  AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\">");//According to demo
            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do Left
            string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            string FarebasisDataL = farebasisdataL;
            string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisL = FarebasisDataL.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataL0 = string.Empty;
            string FarebasisDataL1 = string.Empty;
            string FarebasisDataL2 = string.Empty;

            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }


            //Right
            string segmentIdDataR = AirfaredataR.SegmentidRightdata;
            string FarebasisDataR = farebasisdataR;
            string[] segmentIdsR = segmentIdDataR.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string[] FarebasisR = FarebasisDataR.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
            string FarebasisDataR0 = string.Empty;
            string FarebasisDataR1 = string.Empty;
            string FarebasisDataR2 = string.Empty;

            string segmentIdAtIndexR0 = string.Empty;
            string segmentIdAtIndexR1 = string.Empty;
            string segmentIdAtIndexR2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIdsR.Length == 3)
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
                segmentIdAtIndexR2 = segmentIdsR[2];

                FarebasisDataR0 = FarebasisR[0];
                FarebasisDataR1 = FarebasisR[1];
                FarebasisDataR2 = FarebasisR[2];

            }
            else if (segmentIdsR.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];

                FarebasisDataR0 = FarebasisR[0];
                FarebasisDataR1 = FarebasisR[1];
            }
            else
            {
                segmentIdAtIndexR0 = segmentIdsR[0];

                FarebasisDataR0 = FarebasisR[0];
            }

            foreach (var segment in AirfaredataL.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                count++;
            }

            foreach (var segment in AirfaredataR.segments)
            {
                if (countR == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR0;
                }
                else if (countR == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndexR2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                //fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + segment.designator._DepartureDate + "\" ArrivalTime = \"" + segment.designator._ArrivalDate + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
                fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
                fareRepriceReq.Append("<Connection />");
                fareRepriceReq.Append("</AirSegment>");
                countR++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            //fareRepriceReq.Append("<AirPricingModifiers  InventoryRequestType=\"DirectAccess\">");//According to demo
            fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
                        paxCount++;

                    }
                }
                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"1\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"11\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"1\"/>");
                        paxCount++;
                    }
                }


                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"11\"/>");
                        paxCount++;
                    }
                }





            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIdsL.Length == 3)
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];
                segmentIdAtIndex2 = segmentIdsL[2];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
                FarebasisDataL2 = FarebasisL[2];

            }
            else if (segmentIdsL.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIdsL[0];
                segmentIdAtIndex1 = segmentIdsL[1];

                FarebasisDataL0 = FarebasisL[0];
                FarebasisDataL1 = FarebasisL[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIdsL[0];
                FarebasisDataL0 = FarebasisL[0];
            }
            foreach (var segment in AirfaredataL.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;

                    FarebasisDataL = FarebasisDataL0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                    FarebasisDataL = FarebasisDataL1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                    FarebasisDataL = FarebasisDataL2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }

            if (segmentIdsR.Length == 3)
            {
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];
                segmentIdAtIndexR2 = segmentIdsR[2];

                FarebasisDataR0 = FarebasisR[0];
                FarebasisDataR1 = FarebasisR[1];
                FarebasisDataR2 = FarebasisR[2];

            }
            else if (segmentIdsR.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndexR0 = segmentIdsR[0];
                segmentIdAtIndexR1 = segmentIdsR[1];

                FarebasisDataR0 = FarebasisR[0];
                FarebasisDataR1 = FarebasisR[1];
            }
            else
            {
                segmentIdAtIndexR0 = segmentIdsR[0];

                FarebasisDataR0 = FarebasisR[0];
            }
            foreach (var segment in AirfaredataR.segments)
            {
                if (legKeyCounterR == 0)
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR0;
                    FarebasisDataR = FarebasisR[0];
                }
                else if (legKeyCounterR == 1)
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR1;
                    FarebasisDataR = FarebasisR[1];
                }
                else
                {
                    segmentIdAtIndexR0 = segmentIdAtIndexR2;
                    FarebasisDataR = FarebasisR[2];
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\" FareBasisCode=\"" + FarebasisDataR + "\">");
                //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\"\">");

                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                //fareRepriceReq.Append("<BookingCode Code = \"E\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounterR++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");





            #region old
            //fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            //fareRepriceReq.Append("<soap:Body>");

            //fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" FareRuleType=\"long\" AuthorizedBy = \"Travelport\" CheckOBFees=\"All\" TargetBranch=\"" + _targetBranch + "\">");
            //fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            //fareRepriceReq.Append("<AirItinerary>");
            ////< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            //// to do Left
            //string segmentIdDataL = AirfaredataL.SegmentidLeftdata;
            //string FarebasisDataL = AirfaredataL.FareBasisLeftdata;
            //string[] segmentIdsL = segmentIdDataL.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            //string segmentIdAtIndex0 = string.Empty;
            //string segmentIdAtIndex1 = string.Empty;
            //string segmentIdAtIndex2 = string.Empty;
            //// Checking if the array has at least two elements
            //if (segmentIdsL.Length == 3)
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //    segmentIdAtIndex2 = segmentIdsL[2];

            //}
            //else if (segmentIdsL.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //}
            //else
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //}


            ////Right
            //string segmentIdDataR = AirfaredataR.SegmentidRightdata;
            //string FarebasisDataR = AirfaredataR.FareBasisRightdata;
            //string[] segmentIdsR = segmentIdDataR.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            //string segmentIdAtIndexR0 = string.Empty;
            //string segmentIdAtIndexR1 = string.Empty;
            //string segmentIdAtIndexR2 = string.Empty;
            //// Checking if the array has at least two elements
            //if (segmentIdsR.Length == 3)
            //{
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //    segmentIdAtIndexR1 = segmentIdsR[1];
            //    segmentIdAtIndexR2 = segmentIdsR[2];

            //}
            //else if (segmentIdsR.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //    segmentIdAtIndexR1 = segmentIdsR[1];
            //}
            //else
            //{
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //}

            //foreach (var segment in AirfaredataL.segments)
            //{
            //    if (count == 0)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex0;
            //    }
            //    else if (count == 1)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex2;
            //    }
            //    fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
            //    fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
            //    fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
            //    fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
            //    fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
            //    fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
            //    fareRepriceReq.Append("<Connection />");
            //    fareRepriceReq.Append("</AirSegment>");
            //    count++;
            //}

            //foreach (var segment in AirfaredataR.segments)
            //{
            //    if (countR == 0)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndexR0;
            //    }
            //    else if (countR == 1)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndexR1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndexR2;
            //    }
            //    fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
            //    fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
            //    fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
            //    fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
            //    fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" ");
            //    fareRepriceReq.Append("ParticipantLevel=\"Secure Sell\" LinkAvailability=\"true\" PolledAvailabilityOption=\"Cached status used. Polled avail exists\" OptionalServicesIndicator=\"false\">");
            //    fareRepriceReq.Append("<Connection />");
            //    fareRepriceReq.Append("</AirSegment>");
            //    countR++;
            //}

            //fareRepriceReq.Append("</AirItinerary>");
            //fareRepriceReq.Append("<AirPricingModifiers ETicketability=\"Yes\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
            //fareRepriceReq.Append("<BrandModifiers>");
            //fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            //fareRepriceReq.Append("</BrandModifiers>");
            //fareRepriceReq.Append("</AirPricingModifiers>");
            //if (_GetfligthModel.passengercount != null)
            //{
            //    if (_GetfligthModel.passengercount.adultcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" BookingTravelerRef=\"" + paxCount + "\"/>");
            //        }
            //    }

            //    if (_GetfligthModel.passengercount.childcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" BookingTravelerRef=\"" + paxCount + "\" Age=\"10\"/>");
            //        }
            //    }
            //    if (_GetfligthModel.passengercount.infantcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  PricePTCOnly=\"true\" BookingTravelerRef=\"" + paxCount + "\" Age=\"01\"/>");
            //        }
            //    }
            //}
            //else
            //{

            //    if (_GetfligthModel.adultcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.adultcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  BookingTravelerRef=\"" + paxCount + "\" Code=\"ADT\" />");
            //        }
            //    }



            //    if (_GetfligthModel.childcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.childcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"CNN\" Age=\"10\"/>");
            //        }
            //    }
            //    if (_GetfligthModel.infantcount != 0)
            //    {
            //        for (int i = 0; i < _GetfligthModel.infantcount; i++)
            //        {
            //            paxCount++;
            //            fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" BookingTravelerRef=\"" + paxCount + "\" Code=\"INF\" PricePTCOnly=\"true\" Age=\"01\"/>");
            //        }
            //    }




            //}
            //fareRepriceReq.Append("<AirPricingCommand>");
            //if (segmentIdsL.Length == 3)
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //    segmentIdAtIndex2 = segmentIdsL[2];

            //}
            //else if (segmentIdsL.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //    segmentIdAtIndex1 = segmentIdsL[1];
            //}
            //else
            //{
            //    segmentIdAtIndex0 = segmentIdsL[0];
            //}
            //foreach (var segment in AirfaredataL.segments)
            //{
            //    if (legKeyCounter == 0)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex0;
            //    }
            //    else if (legKeyCounter == 1)
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndex0 = segmentIdAtIndex2;
            //    }
            //    fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\" FareBasisCode=\"" + FarebasisDataL + "\">");
            //    //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\"\">");
            //    fareRepriceReq.Append("<PermittedBookingCodes>");
            //    fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
            //    fareRepriceReq.Append("</PermittedBookingCodes>");
            //    fareRepriceReq.Append("</AirSegmentPricingModifiers>");
            //    legKeyCounter++;
            //}

            //if (segmentIdsR.Length == 3)
            //{
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //    segmentIdAtIndexR1 = segmentIdsR[1];
            //    segmentIdAtIndexR2 = segmentIdsR[2];

            //}
            //else if (segmentIdsR.Length == 2)
            //{
            //    // Accessing elements by index
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //    segmentIdAtIndexR1 = segmentIdsR[1];
            //}
            //else
            //{
            //    segmentIdAtIndexR0 = segmentIdsR[0];
            //}
            //foreach (var segment in AirfaredataR.segments)
            //{
            //    if (legKeyCounterR == 0)
            //    {
            //        segmentIdAtIndexR0 = segmentIdAtIndexR0;
            //    }
            //    else if (legKeyCounterR == 1)
            //    {
            //        segmentIdAtIndexR0 = segmentIdAtIndexR1;
            //    }
            //    else
            //    {
            //        segmentIdAtIndexR0 = segmentIdAtIndexR2;
            //    }
            //    fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\" FareBasisCode=\"" + FarebasisDataR + "\">");
            //    //fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndexR0 + "\"\">");

            //    fareRepriceReq.Append("<PermittedBookingCodes>");
            //    fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
            //    fareRepriceReq.Append("</PermittedBookingCodes>");
            //    fareRepriceReq.Append("</AirSegmentPricingModifiers>");
            //    legKeyCounterR++;
            //}
            //fareRepriceReq.Append("</AirPricingCommand>");
            //fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            //fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");
            #endregion

            //For certification Request
            //1) change the traceid i.e same as LowFare
            //2) airsegment from lowfareresponse
            //3) providercode="1G"
            //4) check segmentif and farebasiscode
            //5) Traceid will be same in all request further

            //string resp = string.Empty;
            //string path = "D:\\pcheck.txt";
            //using (StreamReader reader = new StreamReader(path))
            //{
            //resp = reader.ReadToEnd(); // Reads the entire file content into a string
            //}
            //fareRepriceReq = new StringBuilder();
            //fareRepriceReq.Append(resp);

            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirprice", "SameGDSRT");
            }
            return res;
        }

        public string AirPriceGet_old(string _testURL, StringBuilder fareRepriceReq, SimpleAvailabilityRequestModel _GetfligthModel, string newGuid, string _targetBranch, string _userName, string _password, dynamic Airfaredata, string _AirlineWay)
        {

            int count = 0;
            int paxCount = 0;
            int legcount = 0;
            string origin = string.Empty;
            int legKeyCounter = 0;

            fareRepriceReq = new StringBuilder();
            fareRepriceReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            fareRepriceReq.Append("<soap:Body>");

            fareRepriceReq.Append("<AirPriceReq xmlns=\"http://www.travelport.com/schema/air_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\">");
            fareRepriceReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            fareRepriceReq.Append("<AirItinerary>");
            //< AirSegment Key = "nX2BdBWDuDKAf9mT8SBAAA==" AvailabilitySource = "P" Equipment = "32A" AvailabilityDisplayType = "Fare Shop/Optimal Shop" Group = "0" Carrier = "AI" FlightNumber = "860" Origin = "DEL" Destination = "BOM" DepartureTime = "2024-07-25T02:15:00.000+05:30" ArrivalTime = "2024-07-25T04:30:00.000+05:30" FlightTime = "135" Distance = "708" ProviderCode = "1G" ClassOfService = "T" />


            // to do
            string segmentIdData = Airfaredata.Segmentiddata;
            string[] segmentIds = segmentIdData.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            string segmentIdAtIndex0 = string.Empty;
            string segmentIdAtIndex1 = string.Empty;
            string segmentIdAtIndex2 = string.Empty;
            // Checking if the array has at least two elements
            if (segmentIds.Length == 3)
            {
                segmentIdAtIndex0 = segmentIds[0];
                segmentIdAtIndex1 = segmentIds[1];
                segmentIdAtIndex2 = segmentIds[2];

            }
            else if (segmentIds.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIds[0];
                segmentIdAtIndex1 = segmentIds[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIds[0];
            }


            foreach (var segment in Airfaredata.segments)
            {
                if (count == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (count == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegment Key=\"" + segmentIdAtIndex0 + "\" AvailabilitySource = \"" + segment.designator._AvailabilitySource + "\" Equipment = \"" + segment.designator._Equipment + "\" AvailabilityDisplayType = \"" + segment.designator._AvailabilityDisplayType + "\" ");
                fareRepriceReq.Append("Group = \"" + segment.designator._Group + "\" Carrier = \"" + segment.identifier.carrierCode + "\" FlightNumber = \"" + segment.identifier.identifier + "\" ");
                fareRepriceReq.Append("Origin = \"" + segment.designator.origin + "\" Destination = \"" + segment.designator.destination + "\" ");
                fareRepriceReq.Append("DepartureTime = \"" + Convert.ToDateTime(segment.designator._DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ArrivalTime = \"" + Convert.ToDateTime(segment.designator._ArrivalDate).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") + "\" ");
                fareRepriceReq.Append("FlightTime = \"" + segment.designator._FlightTime + "\" Distance = \"" + segment.designator._Distance + "\" ProviderCode = \"" + segment.designator._ProviderCode + "\" ClassOfService = \"" + segment.designator._ClassOfService + "\" />");
                count++;
            }

            fareRepriceReq.Append("</AirItinerary>");
            fareRepriceReq.Append("<AirPricingModifiers InventoryRequestType=\"DirectAccess\">");
            fareRepriceReq.Append("<BrandModifiers>");
            fareRepriceReq.Append("<FareFamilyDisplay ModifierType=\"FareFamily\"/>");
            fareRepriceReq.Append("</BrandModifiers>");
            fareRepriceReq.Append("</AirPricingModifiers>");
            if (_GetfligthModel.passengercount != null)
            {
                if (_GetfligthModel.passengercount.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"ADT\" Key=\"" + paxCount + "\"/>");
                        paxCount++;
                    }
                }
                if (_GetfligthModel.passengercount.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"CNN\" Key=\"" + paxCount + "\" Age=\"10\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.passengercount.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.passengercount.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Code=\"INF\"  Key=\"" + paxCount + "\" Age=\"01\"/>");
                        paxCount++;
                    }
                }

            }
            else
            {

                if (_GetfligthModel.adultcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.adultcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\"  Key=\"" + paxCount + "\" Code=\"ADT\" />");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.childcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.childcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + paxCount + "\" Code=\"CNN\" Age=\"10\"/>");
                        paxCount++;
                    }
                }

                if (_GetfligthModel.infantcount != 0)
                {
                    for (int i = 0; i < _GetfligthModel.infantcount; i++)
                    {
                        fareRepriceReq.Append("<SearchPassenger xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + paxCount + "\" Code=\"INF\" Age=\"01\"/>");
                        paxCount++;
                    }
                }


            }
            fareRepriceReq.Append("<AirPricingCommand>");
            if (segmentIds.Length == 3)
            {
                segmentIdAtIndex0 = segmentIds[0];
                segmentIdAtIndex1 = segmentIds[1];
                segmentIdAtIndex2 = segmentIds[2];
            }
            else if (segmentIds.Length == 2)
            {
                // Accessing elements by index
                segmentIdAtIndex0 = segmentIds[0];
                segmentIdAtIndex1 = segmentIds[1];
            }
            else
            {
                segmentIdAtIndex0 = segmentIds[0];
            }
            foreach (var segment in Airfaredata.segments)
            {
                if (legKeyCounter == 0)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex0;
                }
                else if (legKeyCounter == 1)
                {
                    segmentIdAtIndex0 = segmentIdAtIndex1;
                }
                else
                {
                    segmentIdAtIndex0 = segmentIdAtIndex2;
                }
                fareRepriceReq.Append("<AirSegmentPricingModifiers AirSegmentRef = \"" + segmentIdAtIndex0 + "\">");
                fareRepriceReq.Append("<PermittedBookingCodes>");
                fareRepriceReq.Append("<BookingCode Code = \"" + segment.designator._ClassOfService + "\"/>");
                fareRepriceReq.Append("</PermittedBookingCodes>");
                fareRepriceReq.Append("</AirSegmentPricingModifiers>");
                legKeyCounter++;
            }
            fareRepriceReq.Append("</AirPricingCommand>");
            fareRepriceReq.Append("<FormOfPayment xmlns = \"http://www.travelport.com/schema/common_v52_0\" Type = \"Credit\" />");
            fareRepriceReq.Append("</AirPriceReq></soap:Body></soap:Envelope>");



            string res = Methodshit.HttpPost(_testURL, fareRepriceReq.ToString(), _userName, _password);
            SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));


            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
            }
            return res;
        }

        public string CreatePNR(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, string? _pricesolution = null)
        {

            int count = 0;
            int icount = 100;
            //int paxCount = 0;
            //int legcount = 0;
            //string origin = string.Empty;
            //int legKeyCounter = 0;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"CNN\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\" TravelerType=\"INF\">");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\">");
                    }


                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    //if (!String.IsNullOrEmpty(paxDetail.FrequentFlierNumber) && paxDetail.FrequentFlierNumber.Length > 5)
                    //{
                    //if (segment_.Bonds[0].Legs[0].AirlineName.Equals("UK"))
                    //{
                    //createPNRReq.Append("<SSR  Key='" + count + "' Type='FQTV' Status='HK' Carrier='UK' FreeText='" + paxDetail.FrequentFlierNumber + "-" + paxDetail.LastName + "/" + paxDetail.FirstName + "" + paxDetail.Title.ToUpper() + "'/>");
                    //}
                    //else
                    //{
                    //  createPNRReq.Append("<com:LoyaltyCard SupplierCode='" + segment_.Bonds[0].Legs[0].AirlineName + "' CardNumber='" + paxDetail.FrequentFlierNumber + "'/>");
                    //}
                    //}
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/03Dec06/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/03Dec06/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        //Domestic
                        //createPNRReq.Append("<Address>");
                        //createPNRReq.Append("<AddressName>Home</AddressName>");
                        //createPNRReq.Append("<Street>20th I Cross</Street>");
                        //createPNRReq.Append("<City>Bangalore</City>");
                        //createPNRReq.Append("<State>KA</State>");
                        //createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        //createPNRReq.Append("<Country>IN</Country>");
                        //createPNRReq.Append("</Address>");
                        //International
                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>DemoSiteAddress</AddressName>");
                        createPNRReq.Append("<Street>Via Augusta 59 5</Street>");
                        createPNRReq.Append("<City>Delhi</City>");
                        createPNRReq.Append("<State>DL</State>");
                        createPNRReq.Append("<PostalCode>111001</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");

                    }

                    if (passengerdetails[i].passengertypecode == "CNN" || passengerdetails[i].passengertypecode == "CHD")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11Dec13/M/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11Dec13/F/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C11 DOB11Dec13</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = "11DEC23";// Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        if (passengerdetails[i].title.ToLower() == "mstr")
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/MI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        else
                        {
                            createPNRReq.Append("<SSR Type=\"DOCS\" Status=\"HK\" FreeText=\"P/IN/G67567/IN/11DEC23/FI/10Oct30/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                        }
                        createPNRReq.Append("<SSR Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        createPNRReq.Append("<SSR Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                //createPNRReq.Append("<AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\" InventoryRequestType=\"DirectAccess\">");
                //createPNRReq.Append("</AirPricingModifiers>");
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                //createPNRReq.Append(Getdetails.PriceSolution.Replace("</air:CancelPenalty>","</air:CancelPenalty><air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\"> </air:AirPricingModifiers>"));
                Getdetails.PriceSolution = Getdetails.PriceSolution.Replace("</air:CancelPenalty>", "</air:CancelPenalty><air:AirPricingModifiers ETicketability=\"Required\" FaresIndicator=\"AllFares\"> </air:AirPricingModifiers>");

                // Define the regex pattern to match any BookingTravelerRef value dynamically
                string pattern = @"<air:PassengerType BookingTravelerRef='(\d+)' Code='INF' Age='(\d+)'/>";
                // Define the replacement string (maintaining the BookingTravelerRef dynamically)
                string replacement = @"<air:PassengerType BookingTravelerRef='$1' Code='INF' PricePTCOnly=""true"" Age='$1'/>";
                // Perform the replacement
                Getdetails.PriceSolution = Regex.Replace(Getdetails.PriceSolution, pattern, replacement);
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");
                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            //}
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            //SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            //SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));
            //if (_AirlineWay.ToLower() == "gdsoneway")
            //{
            //    logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "GDSOneWay");
            //}
            //else
            //{
            //    logs.WriteLogsR("Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "SameGDSRT");
            //}
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(createPNRReq.ToString(), "3-GetPNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetPNRRes", "GDSOneWay", "oneway");
            }
            else
            {
                //    //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                //    if (p == 0)
                //    {
                //        logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                //        logs.WriteLogsR(res, "3-GetAirpriceRes_Left", "GDSRT");
                //    }
                //    else
                //    {
                //        logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                //        logs.WriteLogsR(res, "3-GetAirpriceRes_Right", "GDSRT");
                //    }
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");

            }
            return res;
        }

        public string CreatePNR_abhi(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, string? _pricesolution = null)
        {

            int count = 0;
            int icount = 100;
            //int paxCount = 0;
            //int legcount = 0;
            //string origin = string.Empty;
            //int legKeyCounter = 0;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"CNN\" Age=\"10\" DOB=\"2014-07-25\" >");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\" TravelerType=\"INF\" Age=\"1\" DOB=\"2023-08-25\" >");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    //if (!String.IsNullOrEmpty(paxDetail.FrequentFlierNumber) && paxDetail.FrequentFlierNumber.Length > 5)
                    //{
                    //if (segment_.Bonds[0].Legs[0].AirlineName.Equals("UK"))
                    //{
                    //createPNRReq.Append("<SSR  Key='" + count + "' Type='FQTV' Status='HK' Carrier='UK' FreeText='" + paxDetail.FrequentFlierNumber + "-" + paxDetail.LastName + "/" + paxDetail.FirstName + "" + paxDetail.Title.ToUpper() + "'/>");
                    //}
                    //else
                    //{
                    //  createPNRReq.Append("<com:LoyaltyCard SupplierCode='" + segment_.Bonds[0].Legs[0].AirlineName + "' CardNumber='" + paxDetail.FrequentFlierNumber + "'/>");
                    //}
                    //}
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("<SSR  Key=\"" + count + "\" Type=\"DOCS\"  FreeText=\"P/GB/S12345678/GB/20JUL76/M/01JAN16/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                    string contractNo = string.Empty;
                    if (string.IsNullOrEmpty(contractNo))
                    {
                        contractNo = "CTCM " + passengerdetails[i].mobile + " PAX";
                    }


                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        icount++;
                        createPNRReq.Append("<SSR  Key=\"" + icount + "\" Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        icount++;
                        createPNRReq.Append("<SSR  Key=\"" + icount + "\" Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>Home</AddressName>");
                        createPNRReq.Append("<Street>20th I Cross</Street>");
                        createPNRReq.Append("<City>Bangalore</City>");
                        createPNRReq.Append("<State>KA</State>");
                        createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");
                    }

                    if (passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C04</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF")
                    {
                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");
                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            //}
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            //SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            //SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));
            //if (_AirlineWay.ToLower() == "gdsoneway")
            //{
            //    //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "GDSOneWay");
            //}
            //else
            //{
            //    //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");
            //}

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + fareRepriceReq + "\n\n Response: " + res, "GetAirPrice", "GDSOneWay","oneway");
                logs.WriteLogs(createPNRReq.ToString(), "3-GetPNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(res, "3-GetPNRRes", "GDSOneWay", "oneway");
            }
            else
            {
                //    //logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(fareRepriceReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetAirprice", "GDSRT");
                //    if (p == 0)
                //    {
                //        logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Left", "GDSRT");
                //        logs.WriteLogsR(res, "3-GetAirpriceRes_Left", "GDSRT");
                //    }
                //    else
                //    {
                //        logs.WriteLogsR(fareRepriceReq.ToString(), "3-GetAirpriceReq_Right", "GDSRT");
                //        logs.WriteLogsR(res, "3-GetAirpriceRes_Right", "GDSRT");
                //    }
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");

            }
            return res;
        }
        public string CreatePNR_old(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, string? _pricesolution = null)
        {

            int count = 0;
            //int paxCount = 0;
            //int legcount = 0;
            //string origin = string.Empty;
            //int legKeyCounter = 0;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" key=\"" + count + "\" TravelerType=\"INF\" Age=\"01\" DOB=\"2023-08-25\" >");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"CNN\" Age=\"10\" DOB=\"2014-07-25\" >");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }

                    //Title
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        passengerdetails[i].title = "MR";
                    }
                    else
                    {
                        passengerdetails[i].title = "MSTR";
                    }

                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    //if (!String.IsNullOrEmpty(paxDetail.FrequentFlierNumber) && paxDetail.FrequentFlierNumber.Length > 5)
                    //{
                    //if (segment_.Bonds[0].Legs[0].AirlineName.Equals("UK"))
                    //{
                    //createPNRReq.Append("<SSR  Key='" + count + "' Type='FQTV' Status='HK' Carrier='UK' FreeText='" + paxDetail.FrequentFlierNumber + "-" + paxDetail.LastName + "/" + paxDetail.FirstName + "" + paxDetail.Title.ToUpper() + "'/>");
                    //}
                    //else
                    //{
                    //  createPNRReq.Append("<com:LoyaltyCard SupplierCode='" + segment_.Bonds[0].Legs[0].AirlineName + "' CardNumber='" + paxDetail.FrequentFlierNumber + "'/>");
                    //}
                    //}
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    // createPNRReq.Append("<SSR  Key=\"" + count + "\" Type=\"DOCS\"  FreeText=\"P/GB/S12345678/GB/20JUL76/M/01JAN16/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                    string contractNo = string.Empty;
                    if (string.IsNullOrEmpty(contractNo))
                    {
                        contractNo = "CTCM " + passengerdetails[i].mobile + " PAX";
                    }


                    if (i == 0 && passengerdetails[i].passengertypecode == "ADT")
                    {
                        //createPNRReq.Append("<SSR  Key=\"" + count + "\" Type=\"CTCM\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"1234567890\"/>");
                        //createPNRReq.Append("<SSR  Key=\"" + count+ "\" Type=\"CTCE\" Status=\"HK\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\" FreeText=\"test//ENDFARE.in\"/>");

                        createPNRReq.Append("<Address>");
                        createPNRReq.Append("<AddressName>Home</AddressName>");
                        createPNRReq.Append("<Street>20th I Cross</Street>");
                        createPNRReq.Append("<City>Bangalore</City>");
                        createPNRReq.Append("<State>KA</State>");
                        createPNRReq.Append("<PostalCode>560047</PostalCode>");
                        createPNRReq.Append("<Country>IN</Country>");
                        createPNRReq.Append("</Address>");
                    }

                    if (passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>P-C04</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    string format = Convert.ToDateTime(passengerdetails[i].dateOfBirth).ToString("ddMMMyy");
                    if (passengerdetails[i].passengertypecode == "INF")
                    {
                        createPNRReq.Append("<NameRemark>");
                        createPNRReq.Append("<RemarkData>" + format + "</RemarkData>");
                        createPNRReq.Append("</NameRemark>");
                    }
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<ContinuityCheckOverride xmlns=\"http://www.travelport.com/schema/common_v52_0\">true</ContinuityCheckOverride>");
                createPNRReq.Append("<AgencyContactInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\">");
                createPNRReq.Append("<PhoneNumber CountryCode=\"91\" AreaCode=\"011\" Number=\"46615790\" Location=\"DEL\" Type=\"Agency\"/>");
                createPNRReq.Append("</AgencyContactInfo>");

                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");
                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }

            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            //SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            //SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");
            }
            return res;
        }
        public string CreatePNR_1(string _testURL, StringBuilder createPNRReq, string newGuid, string _targetBranch, string _userName, string _password, string AdultTraveller, string _data, string _Total, string _AirlineWay, string? _pricesolution = null)
        {

            int count = 0;
            //int paxCount = 0;
            //int legcount = 0;
            //string origin = string.Empty;
            //int legKeyCounter = 0;

            createPNRReq = new StringBuilder();
            createPNRReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            createPNRReq.Append("<soap:Body>");
            createPNRReq.Append("<AirCreateReservationReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy = \"Travelport\" TargetBranch=\"" + _targetBranch + "\" ProviderCode=\"1G\" RetainReservation=\"Both\">");
            createPNRReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
            List<passkeytype> passengerdetails = (List<passkeytype>)JsonConvert.DeserializeObject(AdultTraveller, typeof(List<passkeytype>));



            AirAsiaTripResponceModel Getdetails = (AirAsiaTripResponceModel)JsonConvert.DeserializeObject(_data, typeof(AirAsiaTripResponceModel));
            Getdetails.PriceSolution = _pricesolution.Replace("\\", "");

            if (passengerdetails.Count > 0)
            {
                for (int i = 0; i < passengerdetails.Count; i++)
                {
                    if (passengerdetails[i].passengertypecode == "ADT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    else if (passengerdetails[i].passengertypecode == "CHD")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"CNN\" Age=\"10\" DOB=\"2014-07-25\" >");
                    }
                    else if (passengerdetails[i].passengertypecode == "INF" || passengerdetails[i].passengertypecode == "INFT")
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\" TravelerType=\"INF\" Age=\"1\" DOB=\"2023-08-25\" >");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTraveler xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"" + count + "\"  TravelerType=\"ADT\" Age=\"40\" DOB=\"1984-07-25\">");
                    }
                    if (!string.IsNullOrEmpty(passengerdetails[i].middle))
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Middle=\"" + passengerdetails[i].middle.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<BookingTravelerName  First=\"" + passengerdetails[i].first.ToUpper() + "\" Last=\"" + passengerdetails[i].last.ToUpper() + "\" Prefix=\"" + passengerdetails[i].title.ToUpper().Replace(".", "") + "\" />");
                    }
                    if (passengerdetails[i].passengertypecode == "ADT" || passengerdetails[i].passengertypecode == "CHD" || passengerdetails[i].passengertypecode == "CNN")
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[i].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[i].Email + "\" />");
                    }
                    else
                    {
                        createPNRReq.Append("<PhoneNumber Number=\"" + passengerdetails[0].mobile + "\"  />");
                        createPNRReq.Append("<Email EmailID=\"" + passengerdetails[0].Email + "\" />");
                    }

                    //if (!String.IsNullOrEmpty(paxDetail.FrequentFlierNumber) && paxDetail.FrequentFlierNumber.Length > 5)
                    //{
                    //if (segment_.Bonds[0].Legs[0].AirlineName.Equals("UK"))
                    //{
                    //createPNRReq.Append("<SSR  Key='" + count + "' Type='FQTV' Status='HK' Carrier='UK' FreeText='" + paxDetail.FrequentFlierNumber + "-" + paxDetail.LastName + "/" + paxDetail.FirstName + "" + paxDetail.Title.ToUpper() + "'/>");
                    //}
                    //else
                    //{
                    //  createPNRReq.Append("<com:LoyaltyCard SupplierCode='" + segment_.Bonds[0].Legs[0].AirlineName + "' CardNumber='" + paxDetail.FrequentFlierNumber + "'/>");
                    //}
                    //}
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("<SSR  Key=\"" + count + "\" Type=\"DOCS\"  FreeText=\"P/GB/S12345678/GB/20JUL76/M/01JAN16/" + passengerdetails[i].last.ToUpper() + "/" + passengerdetails[i].first.ToUpper() + "\" Carrier=\"" + Getdetails.journeys[0].segments[0].identifier.carrierCode + "\"/>");
                    string contractNo = string.Empty;
                    if (string.IsNullOrEmpty(contractNo))
                    {
                        contractNo = "CTCM " + passengerdetails[i].mobile + " PAX";
                    }
                    //if (!IsDomestic)
                    //{
                    //    if (IsSSR)
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //    else if (ISSSR(segment_.Bonds))
                    //    {
                    //        pnrreq.Append("<com:SSR Type='DOCS'  Key='" + count + "' FreeText='P/" + paxDetail.Nationality + "/" + paxDetail.PassportNo + "/" + paxDetail.Nationality + "/" + paxDetail.DOB.ToString("ddMMMyy") + "/" + PaxGender(paxDetail.Gender) + "/" + paxDetail.PassportExpiryDate.ToString("ddMMMyy") + "/" + paxDetail.FirstName + "/" + paxDetail.LastName + "' Carrier='" + segment_.Bonds[0].Legs[0].AirlineName + "'/>");
                    //    }
                    //}
                    createPNRReq.Append("</BookingTraveler>");
                    count++;
                }
                createPNRReq.Append("<FormOfPayment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"Cash\" Key=\"1\" />");
                createPNRReq.Append(Getdetails.PriceSolution);
                createPNRReq.Append("<ActionStatus xmlns=\"http://www.travelport.com/schema/common_v52_0\" Type=\"ACTIVE\" TicketDate=\"T*\" ProviderCode=\"1G\" />");
                createPNRReq.Append("<Payment xmlns=\"http://www.travelport.com/schema/common_v52_0\" Key=\"2\" Type=\"Itinerary\" FormOfPaymentRef=\"1\" Amount=\"INR" + _Total + "\" />");
                createPNRReq.Append("</AirCreateReservationReq></soap:Body></soap:Envelope>");
            }
            //}
            string res = Methodshit.HttpPost(_testURL, createPNRReq.ToString(), _userName, _password);
            //SetSessionValue("GDSAvailibilityRequest", JsonConvert.SerializeObject(_GetfligthModel));
            //SetSessionValue("GDSPassengerModel", JsonConvert.SerializeObject(_GetfligthModel));
            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + createPNRReq + "\n\n Response: " + res, "GetPNR", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(createPNRReq) + "\n\n Response: " + JsonConvert.SerializeObject(res), "GetPNR", "GDSRT");
            }
            return res;
        }
        public string RetrivePnr(string universalRlcode_, string _testURL, string newGuid, string _targetBranch, string _userName, string _password, string _AirlineWay)
        {
            StringBuilder retrivePnrReq = null;
            string pnrretriveRes = string.Empty;
            try
            {
                retrivePnrReq = new StringBuilder();
                //retrivePnrReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                //retrivePnrReq.Append("<soap:Body>");
                //retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq xmlns:univ=\"http://www.travelport.com/schema/universal_v52_0\" AuthorizedBy=\"ENDFARE\" TargetBranch=\"" + _targetBranch + "\" TraceId=\"" + newGuid + "\">");
                //retrivePnrReq.Append("<com:BillingPointOfSaleInfo xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
                //retrivePnrReq.Append("<univ:ProviderReservationInfo ProviderLocatorCode=\"" + universalRlcode_ + "\" ProviderCode=\"1G\"/>");
                //retrivePnrReq.Append("</univ:UniversalRecordRetrieveReq>");
                //retrivePnrReq.Append("</soap:Body>");
                //retrivePnrReq.Append("</soap:Envelope>");

                retrivePnrReq.Append("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                retrivePnrReq.Append("<soap:Body>");
                retrivePnrReq.Append("<UniversalRecordRetrieveReq xmlns=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" AuthorizedBy=\"Travelport\" TargetBranch=\"" + _targetBranch + "\">");
                retrivePnrReq.Append("<BillingPointOfSaleInfo xmlns=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"uAPI\" />");
                retrivePnrReq.Append("<UniversalRecordLocatorCode>" + universalRlcode_ + "</UniversalRecordLocatorCode>");
                retrivePnrReq.Append("</UniversalRecordRetrieveReq>");
                retrivePnrReq.Append("</soap:Body>");
                retrivePnrReq.Append("</soap:Envelope>");


                //retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq xmlns:univ=\"http://www.travelport.com/schema/universal_v52_0\" AuthorizedBy=\"ENDFARE\" TargetBranch=\"" + _targetBranch + "\" TraceId=\"" + newGuid + "\">");
                //retrivePnrReq.Append("<com:BillingPointOfSaleInfo xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" OriginApplication=\"UAPI\"/>");
                //retrivePnrReq.Append("<univ:ProviderReservationInfo ProviderLocatorCode=\"" + universalRlcode_ + "\" ProviderCode=\"1G\"/>");
                //retrivePnrReq.Append("</univ:UniversalRecordRetrieveReq>");




                //retrivePnrReq.Append("<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>");
                //retrivePnrReq.Append("<s:Header>");
                //retrivePnrReq.Append("<Action s:mustUnderstand='1' xmlns='http://schemas.microsoft.com/ws/2005/05/addressing/none'/>");
                //retrivePnrReq.Append("</s:Header>");
                //retrivePnrReq.Append("<s:Body xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>");
                ////if (IsDomestic)
                ////{
                ////retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq TraceId='" + GetTId(5) + "' TargetBranch='" + _ticketingCredential.Split('|')[0] + "' AuthorizedBy='user'  xmlns:univ='http://www.travelport.com/schema/universal_v46_0'>");
                //retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq xmlns:univ=\"http://www.travelport.com/schema/universal_v52_0\" TraceId=\"" + newGuid + "\" TargetBranch=\"" + _targetBranch + "\" AuthorizedBy=\"ENDFARE\">");
                ////}
                ////else
                ////{
                ////retrivePnrReq.Append("<univ:UniversalRecordRetrieveReq TraceId='" + GetTId(5) + "' TargetBranch='" + _ticketingCredential.Split('|')[0] + "' AuthorizedBy='user'  xmlns:univ='http://www.travelport.com/schema/universal_v46_0'>");
                ////}
                //retrivePnrReq.Append("<com:BillingPointOfSaleInfo OriginApplication=\"UAPI\" xmlns:com=\"http://www.travelport.com/schema/common_v52_0\" />");
                //retrivePnrReq.Append("<univ:ProviderReservationInfo ProviderCode=\"1G\" ProviderLocatorCode=\"" + universalRlcode_ + "\" />");
                //retrivePnrReq.Append("</univ:UniversalRecordRetrieveReq>");
                //retrivePnrReq.Append("</s:Body>");
                //retrivePnrReq.Append("</s:Envelope>");
                pnrretriveRes = Methodshit.HttpPost(_testURL, retrivePnrReq.ToString(), _userName, _password);
            }
            catch (SystemException ex_)
            {
                //Utility.BookingTracker.LogTrackBooking(TransactionId, "[Cloud][TravelPortAPI][PnrRetriveResErr]", pnrretriveRes + "_" + sex_.Message + "_" + sex_.StackTrace, false, "", "");
            }

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                logs.WriteLogs(retrivePnrReq.ToString(), "4-GetRetrievePNRReq", "GDSOneWay", "oneway");
                logs.WriteLogs(pnrretriveRes, "4-GetRetrievePNRRes", "GDSOneWay", "oneway");

                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + retrivePnrReq + "\n\n Response: " + pnrretriveRes, "RetrivePnr", "GDSOneWay");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(retrivePnrReq) + "\n\n Response: " + JsonConvert.SerializeObject(pnrretriveRes), "RetrivePnr", "GDSRT");
            }
            return pnrretriveRes;
        }

        public string GetTicketdata(string universalRlcode_, string _testURL, string newGuid, string _targetBranch, string _userName, string _password, string _AirlineWay)
        {
            StringBuilder retriveTicketPnrReq = null;
            string pnrticketretriveRes = string.Empty;
            try
            {
                retriveTicketPnrReq = new StringBuilder();
                retriveTicketPnrReq.Append("<soap:Envelope xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                retriveTicketPnrReq.Append("<soap:Body>");
                //ReturnInfoOnFail =\"true\" BulkTicket=\"false\"
                retriveTicketPnrReq.Append("<AirTicketingReq TargetBranch=\"" + _targetBranch + "\" TraceId=\"" + newGuid + "\" AuthorizedBy=\"test\"  xmlns=\"http://www.travelport.com/schema/air_v52_0\">");
                retriveTicketPnrReq.Append("<BillingPointOfSaleInfo OriginApplication=\"UAPI\" xmlns=\"http://www.travelport.com/schema/common_v52_0\"/>");
                retriveTicketPnrReq.Append("<AirReservationLocatorCode>" + universalRlcode_ + "</AirReservationLocatorCode>");
                retriveTicketPnrReq.Append("</AirTicketingReq>");
                retriveTicketPnrReq.Append("</soap:Body>");
                retriveTicketPnrReq.Append("</soap:Envelope>");

                pnrticketretriveRes = Methodshit.HttpPost(_testURL, retriveTicketPnrReq.ToString(), _userName, _password);
            }
            catch (SystemException ex_)
            {
                //Utility.BookingTracker.LogTrackBooking(TransactionId, "[Cloud][TravelPortAPI][PnrRetriveResErr]", pnrretriveRes + "_" + sex_.Message + "_" + sex_.StackTrace, false, "", "");
            }

            if (_AirlineWay.ToLower() == "gdsoneway")
            {
                //logs.WriteLogs("URL: " + _testURL + "\n\n Request: " + retriveTicketPnrReq + "\n\n Response: " + pnrticketretriveRes, "RetriveTicketPnr", "GDSOneWay");
                logs.WriteLogs(retriveTicketPnrReq.ToString(), "5-GetRetrieveTicketReq", "GDSOneWay", "oneway");
                logs.WriteLogs(pnrticketretriveRes, "5-GetRetrieveTicketRes", "GDSOneWay", "oneway");
            }
            else
            {
                logs.WriteLogsR("Request: " + JsonConvert.SerializeObject(retriveTicketPnrReq) + "\n\n Response: " + JsonConvert.SerializeObject(pnrticketretriveRes), "RetriveTicketPnr", "GDSRT");
            }
            return pnrticketretriveRes;
        }


        //}
    }
}