using System.Xml.Serialization;
using NsIOControllerSDK;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net.NetworkInformation;


namespace LightControlForXMLRpc

//  适用于Hik的VB2200控制器的自带光源口控制

{

    class LightControl
    {
        private static IntPtr handle = new IntPtr();

        private static int nRet = CIOControllerSDK.MV_OK;

        private static CIOControllerSDK.MV_IO_SERIAL m_stSerial = new CIOControllerSDK.MV_IO_SERIAL();


        public static bool OpenSerial(string serialName)
        {
            LightControl.m_stSerial.strComName = serialName;
            IntPtr ptr = new IntPtr();
            // 1-创建句柄
            if (handle == IntPtr.Zero)
            {
                LightControl.nRet = CIOControllerSDK.MV_IO_CreateHandle_CS(ref ptr);
                if (CIOControllerSDK.MV_OK != nRet || IntPtr.Zero == ptr)
                {
                    Console.WriteLine(LightControl.m_stSerial.strComName + " Creating the handle failed.Err code:" + Convert.ToString(nRet, 16));
                }
                else
                {
                    LightControl.handle = ptr;
                }
            }
            else
            {
                Console.WriteLine("Error. The serial port is open.");
                return false;
            }

            // 2-打开串口
            nRet = CIOControllerSDK.MV_IO_Open_CS(LightControl.handle, ref LightControl.m_stSerial);
            if (CIOControllerSDK.MV_OK != nRet)
            {
                Console.WriteLine("The serial port " + LightControl.m_stSerial.strComName + " is occupied.");
                CIOControllerSDK.MV_IO_Close_CS(LightControl.handle);
                CIOControllerSDK.MV_IO_DestroyHandle_CS(LightControl.handle);
                LightControl.handle = IntPtr.Zero;
                return false;
            }
            else
            {
                Console.WriteLine("The serial port " + LightControl.m_stSerial.strComName + " is available.");
                return true;
            }
        }


        // public static bool SetLightParam(string serialName, int portNumber, UInt16 lightValue, bool lightState, UInt16 durationTime)
        public static bool SetLightParam(List<object> args)
        {
            try
            {
                string serialName = (string)args[0];
                int portNumber = (int)args[1];
                UInt16 lightValue = Convert.ToUInt16(args[2]);
                bool lightState = (bool)args[3];
                int durationTime = (int)args[4];



                bool ret = LightControl.OpenSerial(serialName);
                if (ret)
                {

                    int nRet = CIOControllerSDK.MV_OK;
                    CIOControllerSDK.MV_IO_LIGHT_PARAM stParam = new CIOControllerSDK.MV_IO_LIGHT_PARAM();
                    //协议目前不支持，先不送端口
                    stParam.nPortNumber = (Byte)(CIOControllerSDK.MV_IO_PORT_NUMBER)(portNumber);

                    lightValue = (UInt16)(lightValue < 0 ? 0 : lightValue);
                    lightValue = (UInt16)(lightValue > 100 ? 100 : lightValue);
                    stParam.nLightValue = lightValue;
                    stParam.nLightState = (UInt16)(lightState ? CIOControllerSDK.MV_IO_LIGHTSTATE.MV_IO_LIGHTSTATE_ON : CIOControllerSDK.MV_IO_LIGHTSTATE.MV_IO_LIGHTSTATE_OFF);
                    stParam.nDurationTime = (UInt16)durationTime;
                    Console.WriteLine("nPortNumber:" + stParam.nPortNumber);
                    Console.WriteLine("nLightValue:" + stParam.nLightValue);
                    Console.WriteLine("nLightState:" + stParam.nLightState);
                    Console.WriteLine("nLightEdge:" + stParam.nLightEdge);
                    Console.WriteLine("nDurationTime:" + stParam.nDurationTime);
                    Console.WriteLine("nReserved:" + stParam.nReserved);
                    Console.WriteLine("MV_IO_SetLightParam_CS:" + Convert.ToString(stParam));
                    Console.WriteLine("handle:" + Convert.ToString(LightControl.handle));
                    nRet = CIOControllerSDK.MV_IO_SetLightParam_CS(LightControl.handle, ref stParam);
                    Console.WriteLine("nRet:" + nRet);
                    if (CIOControllerSDK.MV_OK != nRet)
                    {
                        Console.WriteLine("Sending the command of light source settings failed. Error code:" + Convert.ToString(nRet, 16));
                        // return;
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("The command of light source settings is sent.");
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Sending the command failed. Parameters or operations exception occurs.{ex}");
            }
            finally
            {
                LightControl.CloseSerial();

            }
            return false;
        }


        public static void CloseSerial()
        {
            // ch:关闭串口 | en:Disconnect serial 
            if (IntPtr.Zero != LightControl.handle) // SelectedIndex也是从0开始
            {
                Console.WriteLine(LightControl.m_stSerial.strComName + "Handle deleted.");
                CIOControllerSDK.MV_IO_Close_CS(LightControl.handle);
                CIOControllerSDK.MV_IO_DestroyHandle_CS(LightControl.handle);
                LightControl.handle = IntPtr.Zero;

                Console.WriteLine(LightControl.m_stSerial.strComName + " The serial port is closed.");
            }
            else
            {
                Console.WriteLine("Error. The serial port is closed.");
            }
        }

        public static void Main(string[] args)
        {
            // 获取本机所有网络接口信息
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            List<string> userfulIP = new List<string>();
            foreach (NetworkInterface networkInterface in interfaces)
            {
                // 判断网络接口是否为网络接入点
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                     networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                {
                    // 获取网络接口的 IP 地址信息
                    IPInterfaceProperties properties = networkInterface.GetIPProperties();
                    foreach (IPAddressInformation address in properties.UnicastAddresses)
                    {
                        // 排除 IPv6 地址和回环地址
                        if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(address.Address) && !networkInterface.Name.Contains("VMware"))
                        {
                            Console.WriteLine("Interface: {0}, IP Address: {1}", networkInterface.Name, address.Address.ToString());
                            userfulIP.Add(address.Address.ToString());
                        }
                    }
                }
            }
            /*
            */
            using (HttpListener listener = new HttpListener())
            {
                foreach (string ip in userfulIP)
                {
                    listener.Prefixes.Add($"http://{ip}:9090/");
                }
                listener.Start();
                Console.WriteLine("XML-RPC Server is running...");
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    Task.Run(() => ProcessRequest(context));
                }
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                // 读取请求内容
                using (StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    string requestBody = reader.ReadToEnd();
                    Console.WriteLine("Request Body: " + requestBody);

                    // 解析 XML-RPC 请求
                    string responseXml = ParseXmlRpcRequest(requestBody);

                    // 构造响应
                    byte[] buffer = Encoding.UTF8.GetBytes(responseXml);
                    context.Response.ContentType = "text/xml";
                    context.Response.ContentLength64 = buffer.Length;

                    // 发送响应
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing request: " + ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                context.Response.Close();
            }
        }

        static string ParseXmlRpcRequest(string xml)
        {
            // 解析 XML-RPC 请求
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            string methodName = xmlDoc.SelectSingleNode("/methodCall/methodName").InnerText;
            XmlNodeList? paramNodes = xmlDoc.SelectNodes("/methodCall/params/param/value");
            // 在这里解析 XML-RPC 请求的内容，并根据需要进行处理
            List<object> args = new List<object>();

            foreach (XmlNode paramNode in paramNodes)
            {
                string valueType = paramNode.ChildNodes[0].Name;
                string paramValue = paramNode.ChildNodes[0].InnerText;
                Console.WriteLine("param - Type: " + valueType + ", Value: " + paramValue);
                object? value = null;
                switch (valueType)
                {
                    case "int":
                        value = int.Parse(paramValue);
                        args.Add(value);
                        break;
                    case "double":
                        value = double.Parse(paramValue);
                        args.Add(value);
                        break;

                    case "boolean":
                        value = (int.Parse(paramValue) != 0);
                        args.Add(value);
                        break;

                    case "string":
                        value = paramValue;
                        args.Add(value);
                        break;

                    default:
                        Console.WriteLine($"Unknown data type: {valueType}");
                        break;
                }
            }

            // 
            if (methodName == "SetLightParam")
            {
                LightControl.SetLightParam(args);
            }

            // 这里只是一个简单的示例，直接返回一个固定的响应
            string responseXml =  @"<?xml version=""1.0""?>
                                    <methodResponse>
                                    <params>
                                    </params>
                                    </methodResponse>";
            return responseXml;
        }

    }



}



