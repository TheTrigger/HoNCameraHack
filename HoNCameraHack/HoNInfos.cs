using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace HoNCameraHack
{
	public class HoNInfos
	{
		private string[] HoNPossiblePaths { get; } = new string[]
		{
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Heroes of Newerth\game\game_shared.dll",
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Heroes of Newerth\game\game_shared.dll",
		};


		byte[] find_sequence = { 0x00, 0xE0, 0xF6, 0x44 }; // 1975 default
		public float FindValue
		{
			get
			{
				return BitConverter.ToSingle(find_sequence, 0);
			}
			set
			{
				find_sequence = BitConverter.GetBytes(value);
			}
		}


		byte[] patch_sequence = { 0x00, 0x80, 0x09, 0x45 }; // 2200

		/// <summary>
		/// https://gregstoll.com/~gregstoll/floattohex/
		/// </summary>
		public float PatchValue
		{
			get
			{
				return BitConverter.ToSingle(patch_sequence, 0);
			}
			set
			{
				patch_sequence = BitConverter.GetBytes(value);
			}
		}


		private string _dllpath;
		public string DllPath
		{
			get
			{
				if (string.IsNullOrEmpty(_dllpath))
					_dllpath = HoNPossiblePaths.Where(p => File.Exists(p)).First();
				return _dllpath;
			}
		}


		private Version _version;
		public Version Version
		{
			get
			{
				try
				{
					if (_version == null)
					{
						var manifest = Directory.GetParent(Path.GetDirectoryName(DllPath)) + @"\manifest.xml";
						using (var stream = new StreamReader(manifest))
						{
							using (var reader = XmlReader.Create(stream))
							{
								if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "manifest")
								{
									var value = reader.GetAttribute("version");
									var version = Version.Parse(value);

									_version = version;
								}
								else
								{
									throw new FileLoadException("Unable to get version");
								}
							}
						}
					}

					return _version;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}



		/// <summary>
		/// Path game_shared.dll
		/// </summary>
		public List<int> FindOffset()
		{
			var addresses = new List<int>();

			// the fastest way
			var game_shared = File.ReadAllBytes(DllPath);
			for (int i = 0; i < game_shared.Length - find_sequence.Length; i++)
			{
				int k = default;
				while (k != find_sequence.Length && find_sequence[k] == game_shared[i + k++]) ;

				if (k == find_sequence.Length)
				{
					addresses.Add(i);
				}
			}

			return addresses;
		}


		public void Backup()
		{
			File.Copy(DllPath, $"{DllPath}.bak.{DateTime.Now.ToString("yyyyMMdd_HHmm")}", true);
		}


		public bool Patch(int offset)
		{
			try
			{
				this.Backup();

				using (var stream = new FileStream(DllPath, FileMode.Open))
				{
					stream.Seek(offset, SeekOrigin.Begin);

					stream.Write(patch_sequence, 0, patch_sequence.Length);

					return true;
				}
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
