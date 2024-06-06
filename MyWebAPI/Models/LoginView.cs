using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebAPI.Models
{
	public class LoginView
	{
		[DataType(DataType.EmailAddress)]
		public string Usuario { get; set; }
		[DataType(DataType.Password)]
		public string Clave { get; set; }

		public string Rol { get; set; }
	}
}