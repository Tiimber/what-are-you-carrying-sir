using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class PersonBooksConfig {

  public List<Tuple2<string, string>> books = new List<Tuple2<string,string>>();
  
  public PersonBooksConfig(XmlNodeList booksNode) {
    foreach (XmlNode book in booksNode) {
      XmlAttributeCollection bookAttributes = book.Attributes;
      string author = Misc.xmlString(bookAttributes.GetNamedItem("author"));
      string title = Misc.xmlString(bookAttributes.GetNamedItem("title"));
      books.Add(new Tuple2<string, string>(author, title));
    }
  }

}
