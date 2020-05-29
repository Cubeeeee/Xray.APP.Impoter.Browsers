﻿using Gecko;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xray.Tools.ExtractLib.Encode;
using Xray.Tools.SimpleServer;

namespace Xray.Helper.Browser.Music163
{
    /// <summary>
    /// 模板项目
    /// </summary>
    public partial class Form1 : Form
    {
        GeckoWebBrowser browser = new GeckoWebBrowser() { Dock = DockStyle.Fill };

        /// <summary>
        /// 简易HTTP服务器地址
        /// </summary>
        static String APIAddress { get; set; } = ConfigurationManager.AppSettings["Host"];

        const String url = "https://music.163.com/#/playlist?id=2479770216";
        public Form1()
        {
            InitializeComponent();
            this.Text = "浏览器辅助服务";
            panel1.Controls.Add(browser);
            browser.Navigate(url);
            browser.DocumentCompleted += (ss, ee) =>
            {

            };
            //加载完毕
            //method 请求方式  address 请求地址 body请求体
            SimpleServer.StartServer(APIAddress, (method, address, body) =>
            {
                ///响应内容
                String response = String.Empty;
                switch (method.ToUpper())
                {
                    case "GET":

                        break;
                    case "POST":
                        string result = String.Empty; ;

                        if (JsonConvert.DeserializeObject(body) is JObject jobj)
                        {
                            if (address.EndsWith("api/"))
                            {
                                this.Invoke(new Action(() =>
                                {
                                    //使用浏览器执行一些操作
                                    using (AutoJSContext context = new AutoJSContext(browser.Window))
                                    {
                                        try
                                        {
                                            String data = Convert.ToString(jobj.SelectToken("data"));
                                            //删除8-18位
                                            data = data.Substring(0, 8) + data.Substring(18);
                                            //删除换行
                                            data = data.Replace("\n", String.Empty);
                                            var gs = Convert.ToString(jobj.SelectToken("gs"));
                                            var bqi4m = Convert.ToString(jobj.SelectToken("bqi4m"));
                                            ///song?id=412902950 取id前3位
                                            bqi4m = bqi4m.Substring(9, 3);
                                            String js = $"NEJ.P(\"nej.u\").bJI8A(\"{data}\",\"undefined{gs}{bqi4m}\")";
                                            context.EvaluateScript(js, out result);
                                        }
                                        catch (Exception)
                                        {
                                            result = JsonConvert.SerializeObject(new { code = 1,message = "计算失败"});
                                        }
                                  
                                    }
                                }));
                            }
                        }
                        response = EncodeMethod.Encode(EncodeType.UrlDecode, result);
                        break;
                    default:
                        break;
                }
                return response;
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
