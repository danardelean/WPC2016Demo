﻿using System;
namespace SFA.Models
{
	public class Category:BaseModel
	{
		public Category()
		{
			Name = Description = ImageUrl = ParentCategoryId = string.Empty;
			Sequence = 0;
		}

		public string Name { get; set; }

		public string Description { get; set; }

		public string ImageUrl { get; set; }

		public string ParentCategoryId { get; set; }

		public bool HasSubCategories { get; set; }

		public int Sequence { get; set; }
	}
}
