using Colyseus.Schema;

public class Entity : Schema {

	[Type(0, "number")]
	public float x = 0;

	[Type(1, "number")]
	public float y = 0;

    [Type(2, "string")]
	public string type = "";
}