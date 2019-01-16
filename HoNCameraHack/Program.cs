using EasyConsole;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

namespace HoNCameraHack
{
	class Program
	{
		public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		public static string Version => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;


		static void Main(string[] args)
		{
			Console.Title = $"{nameof(HoNCameraHack)} - {Version}";
			var honinfo = new HoNInfos();

			if (!string.IsNullOrEmpty(honinfo.DllPath))
			{
				Console.WriteLine($"DLL Path: {honinfo.DllPath}");
				Console.WriteLine($"HoN Version: {honinfo.Version}\n");

				if (!Program.IsAdministrator)
					Output.WriteLine(ConsoleColor.DarkRed, "You should run this program as Administrator or it may fail\n");

				try
				{
					Console.WriteLine($"Default values: Find => {honinfo.FindValue} / Patch =>  {honinfo.PatchValue} ");

					var addresses = honinfo.FindOffset();

					switch (addresses.Count())
					{
						case 0:
							{
								Output.WriteLine(ConsoleColor.Red, @"/!\ No offset found :( already patched?");
								break;
							}
						case 1:
							{
								var offset = addresses.First();
								Console.WriteLine($"Offset found! {offset:X}");

								if (honinfo.Patch(offset))
								{
									Output.WriteLine(ConsoleColor.Green, "PATCHED!");
								}
								else
								{
									Output.WriteLine(ConsoleColor.Red, "Unable to patch :(");
								}

								break;
							}

						default:
							{
								var menu = new EasyConsole.Menu();

								Output.WriteLine(ConsoleColor.Yellow, @"/!\ Multiple offsets found");
								Console.WriteLine($"recommended to patch the first");
								for (int i = 0; i < addresses.Count(); i++)
								{
									var address = addresses[i];
									menu.Add($"{address:X}", () =>
									{
										if (honinfo.Patch(address))
										{
											Output.WriteLine(ConsoleColor.Green, "PATCHED!");
										}
										else
										{
											Output.WriteLine(ConsoleColor.Red, "Unable to patch :(");
										}

									});
								}

								menu.Display();
								break;
							}
					}
				}
				catch (Exception ex)
				{
					Output.WriteLine(ConsoleColor.DarkRed, $"{ex.GetType()} => {ex.Message}" + Environment.NewLine);
				}
			}
			else
			{
				Output.WriteLine(ConsoleColor.Red, "HoN not found :(");
			}


			Output.WriteLine(ConsoleColor.Gray, "Press any key to close...");
			Console.ReadLine();
		}
	}
}
