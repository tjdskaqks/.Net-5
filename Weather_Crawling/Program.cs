using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


namespace Weather_Crawling
{
   class Program
   {
      const string FIFELINE = "<tr>";
      static void Main(string[] args)
      {
         
         Console.WriteLine($"전국 날씨를 조회 합니다.");
         
         while (true)
         {            
            Console.WriteLine("아래 예)처럼 입력하세요. 잘못된 값을 넣으면 현재 시간으로 조회합니다.");
            Console.WriteLine("일자 및 시간 : 2020.06.30.13");
            string input = Console.ReadLine();

            try
            {
               WebRequest webRequest = WebRequest.Create("http://www.weather.go.kr/weather/observation/currentweather.jsp?auto_man=m&type=t99&reg=100&tm=" + input + "%3A00");
               WebResponse webResponse = webRequest.GetResponse();
               Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

               using (var streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.GetEncoding(51949), true))
               {
                  string abc = streamReader.ReadToEnd();
                  int idx1 = abc.IndexOf("<tbody>");
                  int idx2 = abc.IndexOf("</tbody>");

                  abc = abc.Substring(idx1, idx2 - idx1);

                  int citycount = (abc.Length - abc.Replace(FIFELINE, "").Length) / FIFELINE.Length;
                  Console.WriteLine($"Count : {citycount}");

                  int tridx1 = 0;
                  int tridx2 = 0;
                  List<CityWeather> cities = new List<CityWeather>();

                  for (int i = 0; i < citycount; i++)
                  {
                     if (i == 0)
                     {
                        tridx1 = abc.IndexOf("<tr>");
                        tridx2 = abc.IndexOf("</tr>");
                     }
                     else
                     {
                        tridx1 = abc.IndexOf("<tr>", tridx1);
                        tridx2 = abc.IndexOf("</tr>", tridx1);
                     }
                     string ss = abc.Substring(tridx1, tridx2 - tridx1);
                     tridx1 = tridx2 + 1;

                     cities.Add(GetCityWeather(ss));
                  }
                  foreach (var item in cities)
                  {
                     Console.WriteLine(item.ToString());
                  }
                  Console.WriteLine("조회 성공");
                  break;
               }
            }
            catch (Exception e)
            {
               Console.WriteLine("Error : " + e.Message);
            }
         }
         Console.ReadKey();
      }

      static CityWeather GetCityWeather(string stext)
      {
         int idx1 = 0, idx2 = 0;
         string 도시 = string.Empty, 현재일기 = string.Empty;
         double 현재기온 = 0.0f, 체감온도 = 0.0f, 일강수 = 0.0f;
         byte 습도 = 0;
         for (int i = 0; i < 11; i++)
         {
            string ss1;
            if (i == 0)
            {
               idx1 = stext.IndexOf("\" >");
               idx2 = stext.IndexOf("</a>");
               ss1 = stext.Substring(idx1 + 3, idx2 - idx1 - 3);
               도시 = ss1;
            }
            else
            {
               idx1 = stext.IndexOf("<td>", idx1);
               idx2 = stext.IndexOf("</td>", idx1);
               ss1 = stext.Substring(idx1 + 4, idx2 - idx1 - 4);
            }
            idx1 = idx2 + 1;
            switch (i)
            {
               case 1:
                  현재일기 = ss1 == "&nbsp;" ? "" : ss1;
                  break;
               case 5:
                  현재기온 = Convert.ToDouble(ss1);
                  break;
               case 7:
                  체감온도 = Convert.ToDouble(ss1);
                  break;
               case 8:
                  일강수 = ss1 == "&nbsp;" ? 0.0 : Convert.ToDouble(ss1);
                  break;
               case 9:
                  습도 = Convert.ToByte(ss1);
                  break;
               default:
                  break;
            }
         }

         CityWeather cityWeather = new CityWeather(도시, 현재일기, 현재기온, 체감온도, 일강수, 습도);

         return cityWeather;
      }
      static void print(string ss)
      {
         int idx1 = 0, idx2 = 0;
         string 도시 = string.Empty, 현재일기 = string.Empty;
         double 현재기온 = 0.0f, 체감온도 = 0.0f, 일강수=0.0f;
         byte 습도 = 0;

         for (int i = 0; i < 11; i++)
         {
            string ss1;
            if (i == 0)
            {
               idx1 = ss.IndexOf("\" >");
               idx2 = ss.IndexOf("</a>");
               ss1 = ss.Substring(idx1 + 3, idx2 - idx1 - 3);
               도시 = ss1;
            }
            else
            {
               idx1 = ss.IndexOf("<td>", idx1);
               idx2 = ss.IndexOf("</td>", idx1);
               ss1 = ss.Substring(idx1 + 4, idx2 - idx1 - 4);
            }
            idx1 = idx2 + 1;
            switch (i)
            {
               case 1:
                  현재일기 = ss1 == "&nbsp;" ? "" : ss1;
                  break;
               case 5:
                  현재기온 = Convert.ToDouble(ss1);
                  break;
               case 7:
                  체감온도 = Convert.ToDouble(ss1);
                  break;
               case 8:
                  일강수 = ss1 == "&nbsp;" ? 0.0 : Convert.ToDouble(ss1);
                  break;
               case 9:
                  습도 = Convert.ToByte(ss1);
                  break;
               default:
                  break;
            }
         }
         Console.WriteLine($"지점 : {도시} \t 현재일기 : {현재일기} \t 기온 : {현재기온} \t 체감온도 : {체감온도} \t 일강수 : {일강수} \t 습도 : {습도}");
      }
   }

   

   class CityWeather
   {
      private string sCityName = string.Empty;  // 도시 이름
      private string? sNowDay = null;           // 현재 일기
      private double bNowTemperature = 0.0;     // 현재 기온
      private double bWindChill = 0.0;          // 체감 온도
      private double? bRainfall = null;         // 일강수
      private byte bHumidity = 0;           // 체감온도

      public CityWeather(string sCityName, string sNowDay, double bNowTemperature, double bWindChill, double bRainfall, byte bHumidity)
      {
         this.sCityName = sCityName;
         this.sNowDay = sNowDay;
         this.bNowTemperature = bNowTemperature;
         this.bWindChill = bWindChill;
         this.bRainfall = bRainfall;
         this.bHumidity = bHumidity;
      }
      
      public override string ToString()
      {
         return $"도시 : {sCityName} \t 현재일기 : {sNowDay} \t 기온 : {bNowTemperature}  \t 체감온도 : {bWindChill}\t 일강수 : {bRainfall}\t 습도 : {bHumidity}";
      }
   }
}
