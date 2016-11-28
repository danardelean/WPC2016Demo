using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.Models;
using SFA.Services;
using Xamarin.Forms;
using System.Linq;
using SQLite;
using System.Threading;
using SFA.Helpers;

[assembly: Dependency(typeof(DataService))]

namespace SFA.Services
{
	public class DataService : IDataService
	{

		SQLiteAsyncConnection _conn;

		SQLiteAsyncConnection _Conn
		{
			get
			{
				var path = DependencyService.Get<IDeviceService>().DBLocalPath;
				if (_conn == null)
				{
					using (SQLiteConnection c = new SQLiteConnection(path))
					{
						c.CreateTable<Product>();
						c.CreateTable<Category>();
						c.CreateTable<Account>();
					}
					_conn = new SQLiteAsyncConnection(path);
				}

				return _conn;
			}
		}

		public bool UsesDemoData
		{
			get
			{
				return Settings.UsesDemoData;
			}
			private set
			{
				Settings.UsesDemoData = value;
			}
		}

		public Task<List<Product>> GetProductsAsync(string categoryId)
		{
			return _Conn.Table<Product>()
						.Where(product => product.CategoryId.ToLower() == categoryId.ToLower())
						.OrderBy(product => product.Name).ToListAsync();

		}

		public async Task<List<Category>> GetCategoriesAsync(string parentCategoryId = null)
		{
			if (parentCategoryId == null)
				parentCategoryId = (await _Conn.Table<Category>().Where(category => category.ParentCategoryId == null).FirstAsync()).Id;
			return await _Conn.Table<Category>()
						.Where(category => category.ParentCategoryId.ToLower() == parentCategoryId.ToLower())
						.OrderBy(category => category.Sequence)
						.ToListAsync();
		}

		public Task<List<Account>> GetAccountsAsync(bool includeLeads = false)
		{
			return _Conn.Table<Account>().Where(account => account.IsLead == includeLeads).OrderBy(b => b.Company).ToListAsync();
		}

		public Task SeedLocalDataAsync()
		{
			return _Conn.RunInTransactionAsync((SQLiteConnection tran) =>
			{
				DateTime inceptionDate = DateTime.SpecifyKind(new DateTime(2015, 12, 6, 0, 0, 0), DateTimeKind.Utc); // The DateTime at which these orders were originally generated, in UTC.

				DateTime now = DateTime.UtcNow;

				var today = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0), DateTimeKind.Utc);

				var daysElapsed = (int)(today - inceptionDate).TotalDays;

				var weeksElapsed = (daysElapsed / 7);

				var daysToAdjust = weeksElapsed * 7;


				// setup categories

				// root level category
				var rootCategory = new Category { Name = "Root", HasSubCategories = true, Description = "Root-level category. No products should be direct children of this category.", ParentCategoryId = null, Id = "CC6A4BC6-EC92-494C-A8DE-3A297A7D5E80" };

				// root level categories
				var category_printerKits = new Category { Name = "3D Printer Kits", HasSubCategories = true, Description = "Complete 3D printer kits", Sequence = 1, ParentCategoryId = rootCategory.Id, Id = "84DACE10-D79E-490F-9175-A650DD1FF337" };
				var category_filament = new Category { Name = "3D Filament", HasSubCategories = true, Description = "The plastic filament for printing 3D objects", Sequence = 2, ParentCategoryId = rootCategory.Id, Id = "2FC20A32-EB39-42C6-B029-89C2CF754C03" };
				var category_parts = new Category { Name = "Parts", HasSubCategories = true, Description = "Parts for your 3D printer", Sequence = 3, ParentCategoryId = rootCategory.Id, Id = "92DEC53B-FDD2-4A19-8ED7-3D50F1BD8506" };

				// ptinter categories
				var category_printerKits_abs = new Category { Name = "ABS 3D Printer Kits", Description = "Printer kits that are pre-configured for ABS filament.", Sequence = 1, ParentCategoryId = category_printerKits.Id, Id = "4BB47750-BFDF-4F6E-91E7-78C68CBE3E97" };
				var category_printerKits_pla = new Category { Name = "PLA 3D Printer Kits", Description = "Printer kits that are pre-configured for PLA filament.", Sequence = 2, ParentCategoryId = category_printerKits.Id, Id = "82F6538E-DC93-4E6C-88DE-4E661A0A8EA9" };

				// fialment categories
				var category_filament_abs = new Category { Name = "ABS Filament", Description = "A petroleum-based high-temp plastic, suitable for engineering applications.", Sequence = 1, ParentCategoryId = category_filament.Id, Id = "C8319D33-BE9E-4C46-8781-94D679FF48F1" };
				var category_filament_pla = new Category { Name = "PLA Filament", Description = "A plant-based non-toxic low-warp plastic, suitable for hobbyists.", Sequence = 2, ParentCategoryId = category_filament.Id, Id = "E5513B95-526D-4406-83AC-8A9C91CEED19" };

				// parts categories
				var category_parts_extruders = new Category { Name = "Extruders", Description = "The printhead that extrudes the filament into shapes.", Sequence = 1, ParentCategoryId = category_parts.Id, Id = "490D7254-1E02-415E-B085-0257713D5DEE" };
				var category_parts_buildPlates = new Category { Name = "Build Plates", Description = "The flat surface upon which 3D objects are printed.", Sequence = 2, ParentCategoryId = category_parts.Id, Id = "FEE1E5FF-1A29-405C-BEAF-095BF43DE1E3" };
				var category_parts_coolingFans = new Category { Name = "Cooling Fans", Description = "Extruders get hot. Keep them cool with fans!", Sequence = 3, ParentCategoryId = category_parts.Id, Id = "4049BC10-0658-4C15-8E45-04A90730D458" };
				var category_parts_stepperMotors = new Category { Name = "Stepper Motors", Description = "Precise motors are an important part of 3D printers.", Sequence = 4, ParentCategoryId = category_parts.Id, Id = "A4502104-CC08-47C0-9FEC-C6E95404D8A3" };

				var categories = new List<Category>()
			{
				rootCategory,
				category_printerKits,
				category_filament,
				category_parts,
				category_printerKits_abs,
				category_printerKits_pla,
				category_filament_abs,
				category_filament_pla,
				category_parts_extruders,
				category_parts_buildPlates,
				category_parts_coolingFans,
				category_parts_stepperMotors
			};

				var adjustedCategories = new List<Category>();

				foreach (var c in categories)
				{
					var adujustedCategory = new Category()
					{
						Id = c.Id,
						Name = c.Name,
						HasSubCategories = c.HasSubCategories,
						Description = c.Description,
						Sequence = c.Sequence,
						ParentCategoryId = c.ParentCategoryId,
						CreatedAt = inceptionDate,
						UpdatedAt = inceptionDate.AddDays(daysToAdjust)
					};

					adjustedCategories.Add(adujustedCategory);
				};

				tran.DeleteAll<Category>();

				// add the categories to the context
				foreach (var c in adjustedCategories)
					tran.Insert(c);


				// setup products

				var products = new List<Product>
			{
                // ABS printers
                new Product { Name = "ABS-3DSGNR", Description = "Industrial ABS printer for printing 3D signs.\n\nA Perfect printer for producing high-durability ABS signage for the office or home.", CategoryId = category_printerKits_abs.Id, Price = 324.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer01.jpg", Id = "937D6D2A-4622-45B4-9781-469474252E13" },
				new Product { Name = "ABS-CELL", Description = "Consumer ABS printer specializing in cell phone accessories.\n\nEver wanted to produce your own custom cases for your mobile devices? Now you can.", CategoryId = category_printerKits_abs.Id, Price = 349.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer02.jpg", Id = "77622681-B9D8-474F-B692-69C548A0D200" },
				new Product { Name = "ABS-SCULPT", Description = "A great ABS printer for fine art sculpture duplication.\n\nProduce high-quality scale models of real or 3D-modeled sculptures with this unit.", CategoryId = category_printerKits_abs.Id, Price = 374.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer03.jpg", Id = "C1ED5DA0-903A-431E-87C3-FEF424DE5FD2" },
				new Product { Name = "ABS-COMPLX", Description = "A really complex looking ABS printer.\n\nOur most complex and robust ABS printer model. Capable of printing \"all the things.\"", CategoryId = category_printerKits_abs.Id, Price = 399.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer04.jpg", Id = "07DB9D73-F5CC-45CB-A441-A17BCDC2DA52" },

                // PLA printers
                new Product { Name = "PLA-HOBY", Description = "A great PLA printer for hobbyist projects.\n\nBest used with PLA filament, but may be used with ABS filament if properly vented and outfitted with a high-temp heat plate.", CategoryId = category_printerKits_pla.Id, Price = 324.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer05.jpg", Id = "71001EBD-7EC5-41D8-A279-69E8551A5153" },
				new Product { Name = "PLA-KILR", Description = "PLA printer for custom killer robots.\n\nWith fine multi-axis articulation, this printer is capable of producing parts with fine precision for your robot army. ", CategoryId = category_printerKits_pla.Id, Price = 349.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer06.jpg", Id = "77243AC3-D880-4D30-9AF9-FD4809ABBE4E" },
				new Product { Name = "PLA-DELIKT", Description = "Precision PLA printer for delicate objects.\n\nProduce intricate PLA-based objects with this fine-tuned printer.", CategoryId = category_printerKits_pla.Id, Price = 374.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer07.jpg", Id = "607C7732-498F-436F-B50C-FC5604831C23" },
				new Product { Name = "PLA-REPLCE", Description = "PLA printer for replacement part creation.n\n\nA lightweight portable PLA printer for use in replacement part applications.", CategoryId = category_printerKits_pla.Id, Price = 399.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer08.jpg", Id = "E9E142D6-F6D3-4DEA-AAF2-F6ABDEB826C1" },
				new Product { Name = "PLA-JEWEL", Description = "PLA printer for costume jewelry.\n\nA great classroom printer, suitable for producing plastic trinkets. Kids really like this one!", CategoryId = category_printerKits_pla.Id, Price = 424.99, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/printer09.jpg", Id = "C446C22E-F033-4921-BEA3-F187668D78CC" },

                // ABS filament
                new Product { Name = "FIL-ABS-RED",         Description = "Red ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-red.jpg",                    Id = "77BF10B8-5CDE-44AA-B274-7F0D75B64903" },
				new Product { Name = "FIL-ABS-RED-LIGHT",   Description = "Light red ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-red-light.jpg",        Id = "E1EB0B28-316E-4487-84E2-E3A180E175AF" },
				new Product { Name = "FIL-ABS-RED-DARK",    Description = "Dark red ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-red-dark.jpg",          Id = "DA35980C-AB56-42EF-A12D-FA149ACB9232" },
				new Product { Name = "FIL-ABS-ORG",         Description = "Orange ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-orange.jpg",              Id = "BAC0F2C6-2459-4AD5-98E1-A47402148BF4" },
				new Product { Name = "FIL-ABS-ORG-LIGHT",   Description = "Light orange ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-orange-light.jpg",  Id = "DBACB8F4-5849-4555-A611-CB844BD53D52" },
				new Product { Name = "FIL-ABS-ORG-DARK",    Description = "Dark orange ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-orange-dark.jpg",    Id = "96838B14-DB63-41D2-AA34-F8CE4D6D3475" },
				new Product { Name = "FIL-ABS-YLW",         Description = "Yellow ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-yellow.jpg",              Id = "92AAF9D9-B42A-450E-B84D-A10DFD6A09FC" },
				new Product { Name = "FIL-ABS-YLW-LIGHT",   Description = "Light yellow ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-yellow-light.jpg",  Id = "10EF9239-6063-4507-8FAB-9497FE683C25" },
				new Product { Name = "FIL-ABS-YLW-DARK",    Description = "Dark yellow ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-yellow-dark.jpg",    Id = "AF67F72E-22BC-4927-BE05-3151CB0506B2" },
				new Product { Name = "FIL-ABS-GRN",         Description = "Green ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-green.jpg",                Id = "1ABB13ED-BD99-4E8A-A23E-B690045A0CD7" },
				new Product { Name = "FIL-ABS-GRN-LIGHT",   Description = "Light green ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-green-light.jpg",    Id = "399431F1-0554-46D8-B23E-07E3D9DA2B4B" },
				new Product { Name = "FIL-ABS-GRN-DARK",    Description = "Dark green ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-green-dark.jpg",      Id = "F20F630E-078E-4250-A2B3-FE39FCC1ABD0" },
				new Product { Name = "FIL-ABS-BLU",         Description = "Blue ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-blue.jpg",                  Id = "A8D43274-F824-4745-9110-8BD48950EF63" },
				new Product { Name = "FIL-ABS-BLU-LIGHT",   Description = "Light blue ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-blue-light.jpg",      Id = "A42C905D-4CF0-4432-9E06-D64B82B9E791" },
				new Product { Name = "FIL-ABS-BLU-DARK",    Description = "Dark blue ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-blue-dark.jpg",        Id = "5FEFD3AB-A2E5-4113-98CC-A1D7F88F6353" },
				new Product { Name = "FIL-ABS-IDG",         Description = "Indigo ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-indigo.jpg",              Id = "052A6A07-BBFF-4ADC-ABAD-8C330CB88CDE" },
				new Product { Name = "FIL-ABS-IDG-LIGHT",   Description = "Light indigo ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-indigo-light.jpg",  Id = "40EF24DE-BC23-4E00-9423-7B805E51D33B" },
				new Product { Name = "FIL-ABS-IDG-DARK",    Description = "Dark indigo ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-indigo-dark.jpg",    Id = "56D82F62-8AC8-4648-B346-E67A6888A4FE" },
				new Product { Name = "FIL-ABS-VLT",         Description = "Violet ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-violet.jpg",              Id = "6279B89D-5F2C-4ABE-9E67-5CC8E30C83FB" },
				new Product { Name = "FIL-ABS-VLT-LIGHT",   Description = "Light violet ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-violet-light.jpg",  Id = "479BCDDD-3131-4641-8B68-A0BC9F661C04" },
				new Product { Name = "FIL-ABS-VLT-DARK",    Description = "Dark violet ABS filament, 500 grams.\n\nThis ABS filament is perfect for industrial-grade 3D printing. Compared to PLA, ABS has a longer lifespan, is more impact resistant, and has a more matte look and finish. It is advised that it be used with an enclosed or ventilated printer, as ABS filament produces strong vapors when heated.", Price = 24.99, CategoryId = category_filament_abs.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-violet-dark.jpg",    Id = "66CECE2A-716B-4B9F-9E5E-AD4C617EB449" },

                // PLA filament
                new Product { Name = "FIL-PLA-RED",         Description = "Red PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-red.jpg",                      Id = "AFA05F9A-C090-4982-834B-3DEABE8C8DDB" },
				new Product { Name = "FIL-PLA-RED-LIGHT",   Description = "Light red PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-red-light.jpg",          Id = "AFEF9B74-77F0-4278-BA16-82AC2C0B5A8C" },
				new Product { Name = "FIL-PLA-RED-DARK",    Description = "Dark red PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-red-dark.jpg",            Id = "CD3E5889-CE89-4FE9-B648-2EBDC1D9ACB7" },
				new Product { Name = "FIL-PLA-ORG",         Description = "Orange PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-orange.jpg",                Id = "2AC99E51-A767-41A6-B6D7-B40000A48482" },
				new Product { Name = "FIL-PLA-ORG-LIGHT",   Description = "Light orange PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-orange-light.jpg",    Id = "F896400E-B1F0-4707-AC42-BAEE4B534D67" },
				new Product { Name = "FIL-PLA-ORG-DARK",    Description = "Dark orange PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-orange-dark.jpg",      Id = "4DDE96A9-0297-4773-B6B6-71C87498CB12" },
				new Product { Name = "FIL-PLA-YLW",         Description = "Yellow PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-yellow.jpg",                Id = "98718E46-8166-42CD-973C-B293256909D7" },
				new Product { Name = "FIL-PLA-YLW-LIGHT",   Description = "Light yellow PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-yellow-light.jpg",    Id = "C5EBDA1B-A325-436A-A9BA-ECCAB2814760" },
				new Product { Name = "FIL-PLA-YLW-DARK",    Description = "Dark yellow PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-yellow-dark.jpg",      Id = "C35094C5-57C2-4D6F-B623-F6C9E5DADC4F" },
				new Product { Name = "FIL-PLA-GRN",         Description = "Green PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-green.jpg",                  Id = "9908C1BD-4257-4532-A2C4-7BC1F1F2F070" },
				new Product { Name = "FIL-PLA-GRN-LIGHT",   Description = "Light green PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-green-light.jpg",      Id = "83E0965A-2564-48BC-A56E-219E29B1D296" },
				new Product { Name = "FIL-PLA-GRN-DARK",    Description = "Dark green PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-green-dark.jpg",        Id = "8E933A18-11C9-43BA-B121-850D48685B9F" },
				new Product { Name = "FIL-PLA-BLU",         Description = "Blue PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-blue.jpg",                    Id = "E01184B4-A170-4C15-B2DE-32C0F387B7F2" },
				new Product { Name = "FIL-PLA-BLU-LIGHT",   Description = "Light blue PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-blue-light.jpg",        Id = "F67C85D6-517C-478A-948E-020854BFD815" },
				new Product { Name = "FIL-PLA-BLU-DARK",    Description = "Dark blue PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-blue-dark.jpg",          Id = "F61E1E93-FF60-43FA-8B80-08CC3E37CBD3" },
				new Product { Name = "FIL-PLA-IDG",         Description = "Indigo PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-indigo.jpg",                Id = "57DAAB0C-F3CB-4F80-86C1-56D8EFD51BF9" },
				new Product { Name = "FIL-PLA-IDG-LIGHT",   Description = "Light indigo PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-indigo-light.jpg",    Id = "C6A2A1D7-E312-40BB-9061-920661ADD9DE" },
				new Product { Name = "FIL-PLA-IDG-DARK",    Description = "Dark indigo PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-indigo-dark.jpg",      Id = "70E515EF-5F32-4847-8CB9-0C89600DB775" },
				new Product { Name = "FIL-PLA-VLT",         Description = "Violet PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-violet.jpg",                Id = "B9164838-B14D-4DAE-AC0D-997430D4C5DE" },
				new Product { Name = "FIL-PLA-VLT-LIGHT",   Description = "Light violet PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-violet-light.jpg",    Id = "F176BAC2-8005-416D-B183-603377B4D167" },
				new Product { Name = "FIL-PLA-VLT-DARK",    Description = "Dark violet PLA filament, 500 grams.\n\nThis PLA filament is great for beginner and hobby projects. Compared to ABS filament, PLA demonstrates much less part warping and curling and, for this reason, can be successfully printed without a heated build plate. PLA does not require an enclosed or vented printer because it has low fume output.", Price = 34.99, CategoryId = category_filament_pla.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/filament-violet-dark.jpg",      Id = "7B8E3AC4-B12A-485B-A38D-2C134B1F7879" },

                // extruders
                new Product { Name = "EXTR-001", Description = "1.75 mm filament extruder, 0.35 mm nozzle.", Price = 49.99, CategoryId = category_parts_extruders.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/extruder01.jpg", Id = "D9DA8353-24DA-4B4B-BDCE-162FE0A2E8EF" },
				new Product { Name = "EXTR-002", Description = "2.0 mm filament extruder, 0.50 mm nozzle.", Price = 52.99, CategoryId = category_parts_extruders.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/extruder02.jpg", Id = "115EB7D4-5554-468E-A1DF-F3E38516AF5D" },
				new Product { Name = "EXTR-003", Description = "1.75 mm filament extruder, 0.35 mm nozzle.", Price = 67.99, CategoryId = category_parts_extruders.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/extruder03.jpg", Id = "BEE6786F-E09D-4AF2-8D28-8B7C6F474872" },
				new Product { Name = "EXTR-004", Description = "1.75 mm filament extruder, 0.50 mm nozzle.", Price = 69.99, CategoryId = category_parts_extruders.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/extruder04.jpg", Id = "F7F6A474-0090-4152-B64D-D42289430DFA" },

                // build plates
                new Product { Name = "BLD-PLT-ABS", Description = "A heated build plate for ABS filament projects.\n\nABS filament requires a high-temp build-plate, and this one is just right for those projects.", Price = 15.00, CategoryId = category_parts_buildPlates.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/buildPlate02.jpg", Id = "39AF4F64-33D3-4418-82E3-B6A7BD2C5B6B" },
				new Product { Name = "BLD-PLT-PLA", Description = "A build plate for PLA filament projects.\n\nA low-heat build plate, suitable for PLA projects of any kind.", Price = 15.00, CategoryId = category_parts_buildPlates.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/buildPlate01.jpg", Id = "01A7C873-F126-41CD-831A-60D2B8B3A18B" },

                // cooling fans
                new Product { Name = "FAN-001", Description = "Extruder cooling fan.", Price = 19.00, CategoryId = category_parts_coolingFans.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/fan01.jpg", Id = "7A76BE8C-2345-48FF-BB49-BDE9B3469933" },
				new Product { Name = "FAN-002", Description = "Extruder cooling fan.", Price = 19.00, CategoryId = category_parts_coolingFans.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/fan02.jpg", Id = "54CB35F5-77CB-485F-890E-E47800D940C7" },
				new Product { Name = "FAN-003", Description = "Extruder cooling fan.", Price = 19.00, CategoryId = category_parts_coolingFans.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/fan03.jpg", Id = "9A031303-8A7B-40E5-87C9-2355928DBDE5" },

                // stepper motors
                new Product { Name = "MOT-06V", Description = "6V extruder stepper motor.", Price = 35.00, CategoryId = category_parts_stepperMotors.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/stepperMotor01.jpg", Id = "42B6DFCE-C2A5-465E-B4E6-537F47D8504C" },
				new Product { Name = "MOT-09V", Description = "9V extruder stepper motor.", Price = 35.00, CategoryId = category_parts_stepperMotors.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/stepperMotor02.jpg", Id = "60BE0E74-7786-4287-9BB0-A419D4AADE46" },
				new Product { Name = "MOT-12V", Description = "12V extruder stepper motor.", Price = 35.00, CategoryId = category_parts_stepperMotors.Id, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/stepperMotor03.jpg", Id = "35926C26-9470-4A4D-8DA4-06F1575ABF08" },
			};

				var adjustedProducts = new List<Product>();

				foreach (var p in products)
				{
					var adjustedProduct = new Product()
					{
						Id = p.Id,
						Name = p.Name,
						Description = p.Description,
						Price = p.Price,
						CategoryId = p.CategoryId,
						ImageUrl = p.ImageUrl,
						CreatedAt = inceptionDate,
						UpdatedAt = inceptionDate.AddDays(daysToAdjust)
					};

					adjustedProducts.Add(adjustedProduct);
				};

				tran.DeleteAll<Product>();

				// add the products to the context
				foreach (var p in adjustedProducts)
					tran.Insert(p);

				var accounts = new List<Account>()
			{
                // valid industry values: "None Selected", "Aerospace", "Education", "Electrical", "Entertainment", "Financial Services", "Logistic", "Healthcare", "Manufacturing", "Retail", "Other"

                // customers
                new Account() { Id = "00004363-F79A-44E7-BC32-6128E2EC8401", IsLead = false, Industry = "Electrical", OpportunitySize = 1500, OpportunityStage = "100% - Closed", FirstName = "Joseph", LastName = "Grimes", Company = "GG Mechanical", JobTitle = "Vice President", Email = "jgrimes@ggmechanical.com", Phone = "414-367-4348", Street = "2030 Judah St", City = "San Francisco", PostalCode = "94144", State = "CA", Country = "USA", Latitude = 37.761199, Longitude = -122.483619, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/GGMechanical.jpg", Unit = null, Deleted = false},
				new Account() { Id = "c227bfd2-c6f6-49b5-93ec-afef9eb18d08", IsLead = false, Industry = "Logistic", OpportunitySize = 5000, OpportunityStage = "100% - Closed", FirstName = "Monica", LastName = "Green", Company = "Calcom Logistics", JobTitle = "Director", Email = "mgreen@calcomlogistics.com", Phone = "925-353-8029", Street = "231 3rd Ave", City = "San Francisco", PostalCode = "94118", State = "CA", Country = "USA", Latitude = 37.784312, Longitude = -122.461144, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/CalcomLogistics.jpg", Unit = null, Deleted = false},
				new Account() { Id = "31bf6fe5-18f1-4354-9571-2cdecb0c00af", IsLead = false, Industry = "Education", OpportunitySize = 2200, OpportunityStage = "100% - Closed", FirstName = "Joan", LastName = "Mancum", Company = "Bay Unified School District", JobTitle = "Principal", Email = "joan.mancum@busd.org", Phone = "914-870-7670", Street = "448 Grand Ave", City = "South San Francisco", PostalCode = "94080", State = "CA", Country = "USA", Latitude = 37.656033, Longitude = -122.414383, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/BayUnifiedSchoolDistrict.jpg", Unit = null, Deleted = false},
				new Account() { Id = "45d2ddc0-a8e9-4aea-8b51-2860c708e30d", IsLead = false, Industry = "Manufacturing", OpportunitySize = 1700, OpportunityStage = "100% - Closed", FirstName = "Alvin", LastName = "Gray", Company = "Pacific Cabinetry", JobTitle = "Office Manager", Email = "agray@pacificcabinets.com", Phone = "720-344-7823", Street = "1773 Lincoln St", City = "Santa Clara", PostalCode = "95050", State = "CA", Country = "USA", Latitude = 37.355546, Longitude = -121.955441, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/PacificCabinetry.jpg", Unit = null, Deleted = false},
				new Account() { Id = "c9ebe513-0db2-41d3-b595-20a49454a421", IsLead = false, Industry = "Manufacturing", OpportunitySize = 3000, OpportunityStage = "100% - Closed", FirstName = "Michelle", LastName = "Wilson", Company = "Evergreen Mechanical", JobTitle = "Sales Manager", Email = "mwilson@evergreenmech.com", Phone = "917-245-7975", Street = "208 Jackson St", City = "San Jose", PostalCode = "95112", State = "CA", Country = "USA", Latitude = 37.3489003, Longitude = -121.8940104, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/EvergreenMechanical.jpg", Unit = null, Deleted = false},
				new Account() { Id = "e4029998-5d6e-4ed8-b802-e6f940f307a1", IsLead = false, Industry = "Education", OpportunitySize = 5000, OpportunityStage = "100% - Closed", FirstName = "Jennifer", LastName = "Gillespie", Company = "Peninsula University", JobTitle = "Superintendent", Email = "jgillespie@peninsula.org", Phone = "831-427-6746", Street = "10002 N De Anza Blvd", City = "Cupertino", PostalCode = "95014", State = "CA", Country = "USA", Latitude = 37.3233866, Longitude = -122.0317691, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/PeninsulaUniversity.jpg", Unit = null, Deleted = false},
				new Account() { Id = "2323e8b6-ed1c-44fe-9cff-90dcd97d3bb5", IsLead = false, Industry = "Retail", OpportunitySize = 3000, OpportunityStage = "100% - Closed", FirstName = "Thomas", LastName = "White", Company = "Creative Automotive Group", JobTitle = "Service Manager", Email = "tom.white@creativeauto.com", Phone = "214-865-0771", Street = "1181 Linda Mar Blvd", City = "Pacifica", PostalCode = "94044", State = "CA", Country = "USA", Latitude = 37.585774, Longitude = -122.488545, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/CreativeAutomotiveGroup.jpg", Unit = null, Deleted = false},
				new Account() { Id = "00F8A566-2538-4AF7-AE10-997B61537DC0", IsLead = false, Industry = "Aerospace", OpportunitySize = 5500, OpportunityStage = "100% - Closed", FirstName = "Leon", LastName = "Muks", Company = "Space Y", JobTitle = "President", Email = "leon.muks@spacey.io", Phone = "310-586-0181", Street = "2518 Durant Ave", City = "Berkeley", PostalCode = "94704", State = "CA", Country = "USA", Latitude = 37.867733, Longitude = -122.258311, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/SpaceY.jpg", Unit = null, Deleted = false},
				new Account() { Id = "FEB64319-1222-4C76-A8E5-EDFF84838B43", IsLead = false, Industry = "Logistic", OpportunitySize = 3500, OpportunityStage = "100% - Closed", FirstName = "Floyd", LastName = "Bell", Company = "Netcore", JobTitle = "Procurement", Email = "floyd.bell@netcore.net", Phone = "603-226-4115", Street = "450 15th St", City = "Oakland", PostalCode = "94612", State = "CA", Country = "USA", Latitude = 37.806657, Longitude = -122.274644, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/NetCore.jpg", Unit = null, Deleted = false},
				new Account() { Id = "4FB56717-D2CF-4A5F-894A-C87383A8239D", IsLead = false, Industry = "Manufacturing", OpportunitySize = 1500, OpportunityStage = "100% - Closed", FirstName = "Vanessa", LastName = "Thornton", Company = "Total Sources", JobTitle = "Product Manager", Email = "vanessa.thornton@totalsourcesinc.com", Phone = "419-998-6611", Street = "550 Quarry Rd", City = "San Carlos", PostalCode = "94070", State = "CA", Country = "USA", Latitude = 37.517934, Longitude = -122.267627, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/TotalSources.jpg", Unit = null, Deleted = false},
				new Account() { Id = "89149CDA-5B31-4B15-88D2-6011B028F7AE", IsLead = false, Industry = "Other", OpportunitySize = 2000, OpportunityStage = "100% - Closed", FirstName = "John", LastName = "Boone", Company = "A. L. Price", JobTitle = "Executive Associate", Email = "jboone@alpricellc.com", Phone = "973-579-4610", Street = "233 E Harris Ave", City = "South San Francisco", PostalCode = "94080", State = "CA", Country = "USA", Latitude = 37.651101, Longitude = -122.398316, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/ALPrice.jpg", Unit = null, Deleted = false},
				new Account() { Id = "BFD74C2A-7840-45DD-9C47-13F90DE01F8B", IsLead = false, Industry = "Healthcare", OpportunitySize = 4000, OpportunityStage = "100% - Closed", FirstName = "Ann", LastName = "Temple", Company = "Foxmoor", JobTitle = "Director", Email = "ann.temple@foxmoorinc.com", Phone = "608-821-7667", Street = "1270 San Pablo Ave", City = "Berkeley", PostalCode = "94706", State = "CA", Country = "USA", Latitude = 37.881200, Longitude = -122.296379, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/Foxmoor.jpg", Unit = null, Deleted = false},
				new Account() { Id = "CF568F71-C4B6-45A9-A922-C9E69EF17B49", IsLead = false, Industry = "Other", OpportunitySize = 3000, OpportunityStage = "100% - Closed", FirstName = "Joseph", LastName = "Meeks", Company = "Rose Records", JobTitle = "Manager", Email = "jmeeks@roserecordsllc.com", Phone = "978-628-6826", Street = "28 N 1st St", City = "San Jose", PostalCode = "95113", State = "CA", Country = "USA", Latitude = 37.336638, Longitude = -121.890716, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/RoseRecords.jpg", Unit = null, Deleted = false},
				new Account() { Id = "D81687FD-FE84-4F0F-AFDD-A104B28EC1A7", IsLead = false, Industry = "Retail", OpportunitySize = 2500, OpportunityStage = "100% - Closed", FirstName = "Michelle", LastName = "Herring", Company = "Full Color", JobTitle = "Production Specialist", Email = "michelle.herring@fullcolorus.com", Phone = "201-319-9344", Street = "213 2nd Ave", City = "San Mateo", PostalCode = "94401", State = "CA", Country = "USA", Latitude = 37.566059, Longitude = -122.323836, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/FullColor.jpg", Unit = null, Deleted = false},
				new Account() { Id = "FB580ADA-4381-43EE-ADA1-D072878F08CE", IsLead = false, Industry = "Manufacturing", OpportunitySize = 4500, OpportunityStage = "100% - Closed", FirstName = "Daniel", LastName = "Jones", Company = "Flexus", JobTitle = "Quality Assurance Associate", Email = "daniel.jones@flexusinc.com", Phone = "228-432-8712", Street = "850 Bush St", City = "San Francisco", PostalCode = "94108", State = "CA", Country = "USA", Latitude = 37.789956, Longitude = -122.411338, ImageUrl = "https://xamarincrmv2.blob.core.windows.net/images/Flexus.jpg", Unit = null, Deleted = false},

                // leads
                new Account() { Id = "953d9588-e6be-49cf-881d-68431b8285c3", IsLead = true, Industry = "None Selected", OpportunitySize = 4000, OpportunityStage = "10% - Prospect", FirstName = "Margaret", LastName = "Cargill", Company = "Redwood City Medical Group", JobTitle = "Director", Email = "mcargill@rcmg.org", Phone = "208-816-9793", Street = "1037 Middlefield Road", City = "Redwood City", PostalCode = "94063", State = "CA", Country = null, Latitude = 37.4844764, Longitude = -122.2275844, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "450fe593-433f-4bca-9f39-f2a0e4c64dc6", IsLead = true, Industry = "Manufacturing", OpportunitySize = 2000, OpportunityStage = "75% - Proposal", FirstName = "Benjamin", LastName = "Jones", Company = "JH Manufacturing", JobTitle = "Head of Manufacturing", Email = "ben.jones@jh.com", Phone = "505.562.3086", Street = "2091 Cowper St", City = "Palo Alto", PostalCode = "94306", State = "CA", Country = "USA", Latitude = 37.4359411, Longitude = -122.1395614, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "5c957b8f-6e76-470c-941f-789d12f10a42", IsLead = true, Industry = "Electrical", OpportunitySize = 4000, OpportunityStage = "50% - Value Proposition", FirstName = "Ivan", LastName = "Diaz", Company = "XYZ Robotics", JobTitle = "CEO", Email = "ivan.diaz@xyzrobotics.com", Phone = "406-496-8774", Street = "1960 Mandela Parkway", City = "Oakland", PostalCode = "94607", State = "CA", Country = "USA", Latitude = 37.8145449, Longitude = -122.2890861, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "6FEFF721-2A97-4C0F-AACB-30B1F521ABF6", IsLead = true, Industry = "None Selected", OpportunitySize = 2500, OpportunityStage = "75% - Proposal", FirstName = "Eric", LastName = "Grant", Company = "MMSRI, Inc.", JobTitle = "Senior Manager", Email = "egrant@mmsri.com", Phone = "360-693-2388", Street = "2043 Martin Luther King Jr. Way", City = "Berkeley", PostalCode = "94704", State = "CA", Country = "USA", Latitude = 37.8711182, Longitude = -122.2729422, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "CA0A6161-6898-421D-9F29-A51B60F36BEE", IsLead = true, Industry = "Manufacturing", OpportunitySize = 2000, OpportunityStage = "50% - Value Proposition", FirstName = "Stacey", LastName = "Valdovinos", Company = "Global Manufacturing", JobTitle = "CEO", Email = "svaldovinos@globalmanuf.com", Phone = "440-243-7987", Street = "98 Udayakavi Lane", City = "Danville", PostalCode = "94526", State = "CA", Country = "USA", Latitude = 37.8424459, Longitude = -122.0052429, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "9CD6310F-1439-4898-9F51-EEC96D032CD3", IsLead = true, Industry = "Other", OpportunitySize = 2000, OpportunityStage = "10% - Prospect", FirstName = "Jesus", LastName = "Cardell", Company = "Pacific Marine Supply", JobTitle = "Manager", Email = "jcardella@pacificmarine.com", Phone = "410-745-5521", Street = "1008 Rachele Road", City = "Walnut Creek", PostalCode = "94597", State = "CA", Country = "USA", Latitude = 37.913775, Longitude = -122.07907, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "D5E85894-129F-4F39-A75D-893DAB128ECD", IsLead = true, Industry = "Education", OpportunitySize = 1500, OpportunityStage = "50% - Value Proposition", FirstName = "Wilma", LastName = "Woolley", Company = "Mission School District", JobTitle = "Superintendent", Email = "wwoolley@missionsd.org", Phone = "940-696-1852", Street = "7277 Moeser Lane", City = "El Cerrito", PostalCode = "94530", State = "CA", Country = "USA", Latitude = 37.9144576, Longitude = -122.301514, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "6CF4DE3E-FE50-4860-8E5C-6DCF479D4737", IsLead = true, Industry = "Other", OpportunitySize = 1500, OpportunityStage = "75% - Proposal", FirstName = "Evan", LastName = "Armstead", Company = "City of Richmond", JobTitle = "Board Member", Email = "evan.armstead@richmond.org", Phone = "415-336-2228", Street = "398 23rd St", City = "Richmond", PostalCode = "94804", State = "CA", Country = "USA", Latitude = 37.9368714, Longitude = -122.3475159, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "DAFB9C5C-54A3-4F18-BC01-10AD2491AEC7", IsLead = true, Industry = "Financial Services", OpportunitySize = 1000, OpportunityStage = "10% - Prospect", FirstName = "James", LastName = "Jones", Company = "East Bay Commercial Bank", JobTitle = "Manager", Email = "james.jones@eastbaybank.com", Phone = "313-248-7644", Street = "4501 Pleasanton Wau", City = "Pleasanton", PostalCode = "94556", State = "CA", Country = null, Latitude = 37.7144734, Longitude = -121.8481273, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "AB6F1601-94F3-4E32-A08A-089B5B52DA36", IsLead = true, Industry = "Financial Services", OpportunitySize = 4500, OpportunityStage = "50% - Value Proposition", FirstName = "Douglas", LastName = "Greenly", Company = "Bay Tech Credit Union", JobTitle = "Vice President", Email = "d.greenly@baytechcredit.com", Phone = "201-929-0094", Street = "2267 Alameda Ave", City = "Alameda", PostalCode = "94501", State = "CA", Country = "USA", Latitude = 37.7649549215216, Longitude = -122.245887410091, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "70EB3223-4ED2-4FE2-9AC1-F72B474FF05F", IsLead = true, Industry = "Entertainment", OpportunitySize = 1000, OpportunityStage = "10% - Prospect", FirstName = "Brent", LastName = "Mason", Company = "Rockridge Hotel", JobTitle = "Concierge", Email = "brent.mason@rockridgehotel.com", Phone = "940-482-7759", Street = "1960 Mandela Parkway", City = "Oakland", PostalCode = "94607", State = "CA", Country = "USA", Latitude = 37.8145449, Longitude = -122.2890861, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "A5A8F111-FE08-4478-A90B-222F4BA033DD", IsLead = true, Industry = "Other", OpportunitySize = 1000, OpportunityStage = "10% - Prospect", FirstName = "Richard", LastName = "Hogan", Company = "Marin Luxury Senior Living", JobTitle = "Customer Care", Email = "rhogan@marinseniorliving.com", Phone = "978-658-7545", Street = "674 Tiburon Blvd", City = "Belvedere Tiburon", PostalCode = "94920", State = "CA", Country = "USA", Latitude = 37.890031, Longitude = -122.478682, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "6348C5F4-2073-4868-959C-D1650FD8C186", IsLead = true, Industry = "Aerospace", OpportunitySize = 2000, OpportunityStage = "10% - Prospect", FirstName = "Daniel", LastName = "Granville", Company = "Cityview Consulting", JobTitle = "Consultant", Email = "dgranville@cityviewconsulting.com", Phone = "330-616-7467", Street = "300 Spencer Ave", City = "Sausalito", PostalCode = "94965", State = "CA", Country = null, Latitude = 37.851951, Longitude = -122.489919, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "303A5E88-E91D-43ED-9391-FDE9F7C03A66", IsLead = true, Industry = "Entertainment", OpportunitySize = 1000, OpportunityStage = "50% - Value Proposition", FirstName = "Margaret", LastName = "Kidd", Company = "Marin Cultural Center", JobTitle = "President", Email = "mkidd@marincultural.org", Phone = "406-784-0602", Street = "106 Throckmorton Ave", City = "Mill Valley", PostalCode = "94941", State = "CA", Country = null, Latitude = 7.906235, Longitude = -122.548516, ImageUrl = null, Unit = null, Deleted = false},
				new Account() { Id = "0782C981-F003-44A4-87D1-771D3C6EB6B3", IsLead = true, Industry = "Other", OpportunitySize = 2500, OpportunityStage = "10% - Prospect", FirstName = "Leo", LastName = "Parson", Company = "San Rafel Chamber of Commerce", JobTitle = "Board Member", Email = "leo.parson@sanrafaelcoc.org", Phone = "773-991-5214", Street = "199 Clorinda Ave", City = "San Rafael", PostalCode = "94901", State = "CA", Country = null, Latitude = 37.967382, Longitude = -122.539094, ImageUrl = null, Unit = null, Deleted = false},
			};

				var adjustedAccounts = new List<Account>();

				foreach (var a in accounts)
				{
					var adjustedAccount = new Account()
					{
						Id = a.Id,
						IsLead = a.IsLead,
						Industry = a.Industry,
						OpportunitySize = a.OpportunitySize,
						OpportunityStage = a.OpportunityStage,
						FirstName = a.FirstName,
						LastName = a.LastName,
						Company = a.Company,
						JobTitle = a.JobTitle,
						Email = a.Email,
						Phone = a.Phone,
						Street = a.Street,
						City = a.City,
						State = a.State,
						PostalCode = a.PostalCode,
						Country = a.Country,
						Latitude = a.Latitude,
						Longitude = a.Longitude,
						ImageUrl = a.ImageUrl,
						Unit = a.Unit,
						Deleted = a.Deleted,
						CreatedAt = inceptionDate,
						UpdatedAt = inceptionDate.AddDays(daysToAdjust)
					};

					adjustedAccounts.Add(adjustedAccount);
				};

				tran.DeleteAll<Account>();

				// add the accounts to the context
				foreach (var a in adjustedAccounts)
					tran.Insert(a);

				UsesDemoData = true;
			});

		}

	
	}
}
