// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Voting.Lib.Ech.Ech0045_4_0.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping;

public static class SwissPersonExtensionMapping
{
    public static SwissPersonExtension? GetExtension(object extension)
    {
        if (!(extension is XmlNode[] extensionChildNodes))
        {
            return null;
        }

        var extensionNode = extensionChildNodes.FirstOrDefault()?.ParentNode
            ?? throw new InvalidOperationException("Swiss person extension child node has no parent node");

        using var reader = new StringReader(extensionNode.OuterXml);
        return DeserializeXmlNode<SwissPersonExtension>(reader);
    }

    private static T? DeserializeXmlNode<T>(TextReader reader)
    {
        var serializer = new XmlSerializer(typeof(T));
        return (T?)serializer.Deserialize(reader);
    }
}
