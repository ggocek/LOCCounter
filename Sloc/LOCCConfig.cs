using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Sloc
{
	/// <summary>
	/// A class for managing the specification of comment delimiters in the app.config file.
	/// </summary>
	public class LOCCElement : ConfigurationElement
	{
		public LOCCElement()
		{
			this.Name = "c";
			this.Value = "//, /* */";
		}

		[ConfigurationProperty("name", DefaultValue = "c", IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)this["name"]; }
			set { this["name"] = value; }
		}
		[ConfigurationProperty("value", DefaultValue = "//, /* */", IsKey = true, IsRequired = true)]
		public string Value
		{
			get { return (string)this["value"]; }
			set { this["value"] = value; }
		}
	}

	/// <summary>
	/// A class for managing the specification of comment delimiters in the app.config file.
	/// </summary>
	public class LOCCElementCollection : ConfigurationElementCollection
	{
		public LOCCElementCollection()
		{
			LOCCElement locce = (LOCCElement)CreateNewElement();
			Add(locce);
		}

		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new LOCCElement();
		}

		protected override Object GetElementKey(ConfigurationElement element)
		{
			return ((LOCCElement)element).Name;
		}

		public LOCCElement this[int index]
		{
			get
			{
				return (LOCCElement)BaseGet(index);
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		new public LOCCElement this[string Name]
		{
			get
			{
				return (LOCCElement)BaseGet(Name);
			}
		}

		public int IndexOf(LOCCElement locce)
		{
			return BaseIndexOf(locce);
		}

		public void Add(LOCCElement url)
		{
			BaseAdd(url);
		}

		protected override void BaseAdd(ConfigurationElement element)
		{
			BaseAdd(element, false);
		}
		public void Remove(LOCCElement url)
		{
			if (BaseIndexOf(url) >= 0)
				BaseRemove(url.Name);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Remove(string name)
		{
			BaseRemove(name);
		}

		public void Clear()
		{
			BaseClear();
		}
	}

	/// <summary>
	/// A class for managing the specification of comment delimiters in the app.config file.
	/// </summary>
	public class LOCCConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("extensions", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(LOCCElementCollection),
			AddItemName = "add",
			ClearItemsName = "clear",
			RemoveItemName = "remove")]
		public LOCCElementCollection Extensions
		{
			get
			{
				LOCCElementCollection loccCollection =
					(LOCCElementCollection)base["extensions"];
				return loccCollection;
			}
		}
	}
}
