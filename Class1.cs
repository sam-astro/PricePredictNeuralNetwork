using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;

namespace TechnetSamples
{
	class Program
	{
		static void Upload()
		{
			WebClient webClient = new WebClient();

			NameValueCollection formData = new NameValueCollection();

			webClient.Dispose();

			string URL = "http://achillium.us.to/neuralnetdata/uploadweights.php";
			string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
			System.Net.WebRequest webRequest = System.Net.WebRequest.Create(URL);

			webRequest.Method = "POST";
			webRequest.ContentType = "multipart/form-data; boundary=" + boundary;

			string FilePath = ".\\dat\\weightpersistence.dat";
			formData.Clear();
			formData["name"] = "weightpersistence.dat";
			formData["ReplaceAll"] = "false";

			Stream postDataStream = GetPostStream(FilePath, formData, boundary);

			webRequest.ContentLength = postDataStream.Length;
			Stream reqStream = webRequest.GetRequestStream();

			postDataStream.Position = 0;

			byte[] buffer = new byte[1024];
			int bytesRead = 0;

			while ((bytesRead = postDataStream.Read(buffer, 0, buffer.Length)) != 0)
			{
				reqStream.Write(buffer, 0, bytesRead);
			}

			postDataStream.Close();
			reqStream.Close();

			StreamReader sr = new StreamReader(webRequest.GetResponse().GetResponseStream());
			string Result = sr.ReadToEnd();
		}

		private static Stream GetPostStream(string filePath, NameValueCollection formData, string boundary)
		{
			Stream postDataStream = new System.IO.MemoryStream();

			//adding form data
			string formDataHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
			"Content-Disposition: form-data; name=\"{0}\";" + Environment.NewLine + Environment.NewLine + "{1}";

			foreach (string key in formData.Keys)
			{
				byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(formDataHeaderTemplate,
				key, formData[key]));
				postDataStream.Write(formItemBytes, 0, formItemBytes.Length);
			}

			//adding file data
			FileInfo fileInfo = new FileInfo(filePath);

			string fileHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
			"Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
			Environment.NewLine + "Content-Type: application/vnd.ms-excel" + Environment.NewLine + Environment.NewLine;

			byte[] fileHeaderBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(fileHeaderTemplate,
			"UploadCSVFile", fileInfo.FullName));

			postDataStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

			FileStream fileStream = fileInfo.OpenRead();

			byte[] buffer = new byte[1024];

			int bytesRead = 0;

			while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
			{
				postDataStream.Write(buffer, 0, bytesRead);
			}

			fileStream.Close();

			byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--");
			postDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

			return postDataStream;
		}
	}
}