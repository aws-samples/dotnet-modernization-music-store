<Query Kind="Statements">
  <Connection>
    <ID>1dd12535-d16f-44db-b473-13cf406fe9e0</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>.</Server>
    <Database>MusicStore</Database>
  </Connection>
  <Output>DataGrids</Output>
  <Namespace>System.Dynamic</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

IQueryable GetAlbums() =>
	from album in Albums
	select new { id = album.AlbumId, album.AlbumId, album.Title, album.Price, album.AlbumArtUrl };

ToElasticSearchJson(GetAlbums, "albums").Dump();

string ToElasticSearchJson(Func<IEnumerable> query, string esIndexName, string idPropertyName = "id", string esTypeName = "_doc") =>
	string.Join("\n", ToElasticSearchJsonItems(query, esIndexName, idPropertyName, esTypeName, "delete", "create"));

IEnumerable<string> ToElasticSearchJsonItems(Func<IEnumerable> query, string esIndexName, string idPropertyName, string esTypeName, params string[] esCommands)
{
	foreach (var item in query())
	{
		(string id, ExpandoObject obj) = ObjectLessId(item, idPropertyName);
		foreach (object esCommand in CreateEsCommands(id, esIndexName, esTypeName, esCommands))
			yield return JsonSerializer.Serialize(esCommand);
		if (esCommands.Contains("create") || esCommands.Contains("update"))
			yield return JsonSerializer.Serialize(obj);
	}
	yield return "\n";
}

(string, ExpandoObject) ObjectLessId(object item, string idPropertyName)
{
	string id = null;
	var expando = new ExpandoObject();
	var dictionary = (IDictionary<string, object>)expando;

	foreach (var property in item.GetType().GetProperties())
		if (idPropertyName == property.Name)
			id = property.GetValue(item).ToString();
		else
			dictionary.Add(property.Name, property.GetValue(item));

	return (id, expando);
}

IEnumerable<object> CreateEsCommands(string id, string esIndexName, string esTypeName, string[] commands)
{
	foreach (string command in commands)
	{
		object cmd;
		switch (command)
		{
			case "delete":
				cmd = new { delete = CreateEsCmdArg(id, esIndexName, esTypeName) };
				break;
			case "create":
				cmd = new { create = CreateEsCmdArg(id, esIndexName, esTypeName) };
				break;
			default:
				throw new ArgumentException($"Command \"{command}\" is invalid");
		}
		yield return cmd;
	}
}

object CreateEsCmdArg(string id, string esIndexName, string esTypeName) =>
	new { _index = esIndexName, _type = esTypeName, _id = id, };
