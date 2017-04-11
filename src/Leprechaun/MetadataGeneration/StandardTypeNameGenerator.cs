﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Leprechaun.MetadataGeneration
{
	/// <summary>
	/// Generates class names and relative namespaces for code generation types
	/// </summary>
	public class StandardTypeNameGenerator : ITypeNameGenerator
	{
		private readonly string _namespaceRoot;

		public StandardTypeNameGenerator(string namespaceRootPath)
		{
			if(namespaceRootPath == "/") throw new NotSupportedException("Namespace root cannot be /, please use a sub-path e.g. /sitecore/templates");

			_namespaceRoot = namespaceRootPath;
		}

		/// <summary>
		/// Calculates a relative namespace and type name for a template based on its relative path from the root namespace path
		/// </summary>
		public virtual string GetFullTypeName(string fullPath)
		{
			string name = fullPath.Replace(_namespaceRoot, string.Empty).Trim('/').Replace('/', '.');

			var nameParts = name.Split('/');

			for (int cnt = 0; cnt < nameParts.Length; cnt++)
			{
				string namePart = nameParts[cnt];
				int v;
				if (int.TryParse(namePart.Substring(0, 1), out v))
				{
					namePart = "_" + namePart;
				}

				nameParts[cnt] = namePart;
			}

			name = string.Join(".", nameParts);

			if (name.Contains("."))
			{
				string typeName = name.Substring(name.LastIndexOf('.') + 1);
				string namespaceName = ConvertToIdentifier(name.Substring(0, name.LastIndexOf('.')));

				name = ConvertToIdentifier(string.Concat(namespaceName, ".", typeName));
			}

			return name;
		}

		/// <summary>
		/// Converts a string into a valid .NET identifier
		/// </summary>
		public virtual string ConvertToIdentifier(string name)
		{
			// Desnakeify case if it exists (e.g. foo_bar -> "foo bar")
			name = name.Replace("_", " ");

			// Uppercase any non-capitalized words (e.g. 'lord flowers' -> 'Lord Flowers')
			// this makes identifiers Pascal Case as .NET expects
			name = Regex.Replace(name, "^([a-z])", match => match.Value.ToUpperInvariant()); // first letter
			name = Regex.Replace(name, " ([a-z])", match => match.Value.ToUpperInvariant()); // subsequent words

			// allow for fields that start with a number (this is not allowed as an identifier)
			if (char.IsDigit(name[0]))
				name = "_" + name;

			// replace invalid chars for an identifier with nothing (removes spaces, etc)
			return Regex.Replace(name, "[^a-zA-Z0-9_\\.]+", string.Empty);
		}
	}
}