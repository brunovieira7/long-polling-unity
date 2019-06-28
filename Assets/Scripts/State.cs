using Colyseus.Schema;

public class State : Schema {
    [Type(0, "map", typeof(MapSchema<Entity>))]
	public MapSchema<Entity> entities = new MapSchema<Entity>();

	[Type(1, "array", "number")]
	public ArraySchema<float> arrayOfNumbers = new ArraySchema<float>();
}