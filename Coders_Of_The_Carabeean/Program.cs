using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player {
	static void Main(string[] args) {
		
		int turn = 0;
		Barbanera barbanera = new Barbanera();

		// game loop
		while (true) {
			// INIZIO TURNO
			List<Ship> ships = new List<Ship>();
			List<Barrel> barrels = new List<Barrel>();
			List<Mine> mines = new List<Mine>();
			List<CannonSplash> cannons = new List<CannonSplash>();
			turn++;

			int myShipCount = int.Parse(Console.ReadLine()); // the number of remaining ships
			int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. ships, mines or cannonballs)

			// Legge tutti gli input
			for (int i = 0; i < entityCount; i++) {
				string inputLine = Console.ReadLine();
				//Console.Error.WriteLine(inputLine);
				string[] inputs = inputLine.Split(' ');
				int entityId = int.Parse(inputs[0]);
				string entityType = inputs[1];
				int x = int.Parse(inputs[2]);
				int y = int.Parse(inputs[3]);
				int arg1 = int.Parse(inputs[4]);
				int arg2 = int.Parse(inputs[5]);
				int arg3 = int.Parse(inputs[6]);
				int arg4 = int.Parse(inputs[7]);

				switch (entityType) {
					case "SHIP":
						Ship newShip = new Ship(entityId, x, y, arg1, arg2, arg3, arg4); // Creates a SHIP
						ships.Add(newShip);
						break;
					case "BARREL":
						Barrel barrel = new Barrel(entityId, x, y, arg1);
						barrels.Add(barrel);
						break;
					case "MINE":
						Mine mine = new Mine(entityId, x, y);
						mines.Add(mine);
						break;
					case "CANNONBALL":
						Ship shootingShip = ships.FirstOrDefault(ship => ship.entityId == arg1);
						CannonSplash cannon = new CannonSplash(shootingShip, arg2, entityId, x, y);
						break;

				}
			}
			//------------------------------------------------
			Game game = new Game(ships, barrels, mines, cannons, turn);
			string myMove = barbanera.Think(game);
			Console.WriteLine(myMove);
		}
	}
}

#region Data Classes

public class Barbanera {

	public string Think(Game game) {
		string result = "";

		List<Ship> myShips = game.GetMyShips();
		for (int i = 0; i < myShips.Count; i++) {
			Ship currentShip = myShips[i];

			//Find closest ship
			Ship ce = currentShip.GetClosestEnemy(game);
			Console.Error.WriteLine("Closest enemy is " + ce);
			Ship closestEnemy = null;
			float minShipDistance = float.MaxValue;
			foreach (Ship enemyShip in game.GetOpponentShips()) {
				float distance = currentShip.GetDistanceTo(enemyShip);
				if (distance < minShipDistance) {
					closestEnemy = enemyShip;
					minShipDistance = distance;
				}
			}
			Console.Error.WriteLine("Closest enemy is " + closestEnemy);
			//--------------------------------

			//Find closest barrel
			Barrel cb = currentShip.GetClosest<Barrel>(game);
			Console.Error.WriteLine("Closest barrel is " + cb);
			Barrel closestBarrel = null;
			float minBarrelDistance = float.MaxValue;
			foreach (Barrel barrel in game.barrels) {
				float distance = currentShip.GetDistanceTo(barrel);
				if (distance < minBarrelDistance) {
					closestBarrel = barrel;
					minBarrelDistance = distance;
				}
			}
			Console.Error.WriteLine("Closest barrel is " + closestBarrel);
			//-----------------


			if (game.barrels.Count == 0) {
				if (game.turns % 2 == 0) {
					Console.Error.WriteLine("No more barrels");
					int casx = new Random().Next(0, 5);
					int casy = new Random().Next(0, 5);
					result += "MOVE " + casx + " " + casy+"\n";
				} else {
					result += "FIRE " + closestEnemy.pos.x + " " + closestEnemy.pos.y + "\n";
				}

			} else if (currentShip.GetDistanceTo(closestEnemy) <= 5) {
				result += ("FIRE " + closestEnemy.pos.x + " " + closestEnemy.pos.y + "\n");
			} else {
				result += ("MOVE " + closestBarrel.pos.x + " " + closestBarrel.pos.y + "\n");
			}
		}
		return result.Substring(0, result.Length-1);
	}


}

public enum Owner {
	Enemy=0, Player=1, Neutral=2
}

public class Game {

	public List<Ship> ships = new List<Ship>();
	public List<Barrel> barrels = new List<Barrel>();
	public List<Mine> mines = new List<Mine>();
	public List<CannonSplash> cannons = new List<CannonSplash>();
	public int turns;

	public List<EntitySpazioTemporale> entities {
		get {
			return ships.Cast<EntitySpazioTemporale>()
				.Concat(barrels.Cast<EntitySpazioTemporale>())
				.Concat(mines.Cast<EntitySpazioTemporale>())
				.Concat(cannons.Cast<EntitySpazioTemporale>())
				.ToList();
		}
	}


	public Game(List<Ship> ships, List<Barrel> barrels, List<Mine> mines, List<CannonSplash> cannons, int turns) {
		this.ships = ships;
		this.barrels = barrels;
		this.mines = mines;
		this.cannons = cannons;
		this.turns = turns;
	}

	public List<Ship> GetShips(int owner) {
		var result = from ship in ships
					 where ship.owner == (Owner) owner
					 select ship;

		return result.ToList();
	}

	public List<Ship> GetMyShips() {
		return GetShips(1);
	}

	public List<Ship> GetOpponentShips() {
		return GetShips(0);
	}
}

public class Position {
	public int x, y;

	public Position(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public override string ToString() {
		return string.Format("({0}, {1})", x, y);
	}
}

public abstract class Singularity {
	public int entityId;
	public Owner owner;

	public Singularity(int entityId, Owner owner = Owner.Neutral) {
		this.entityId = entityId;
		this.owner = owner;
	}
}

public abstract class EntitySpazioTemporale : Singularity {
	public Position pos;

	public EntitySpazioTemporale(int entityId, int x, int y) : base(entityId) {
		this.pos = new Position(x, y);
	}

	public float GetDistanceTo(EntitySpazioTemporale entity) {
		return Math.Abs(this.pos.x - entity.pos.x) + Math.Abs(this.pos.y - entity.pos.y);
	}

	public List<T> GetSorted<T>(List<T> entities) where T : EntitySpazioTemporale {
		List<T> result = new List<T>(entities);
		result.Sort((T e1, T e2) => {
			return this.GetDistanceTo(e1).CompareTo(this.GetDistanceTo(e2));
		});
		return result;
	}

	public List<T> GetSortedDescending<T>(List<T> entities) where T : EntitySpazioTemporale {
		List<T> result = new List<T>(entities);
		result.Reverse();
		return result;
	}

	public EntitySpazioTemporale GetClosest(EntitySpazioTemporale entity1, EntitySpazioTemporale entity2) {
		float distTo1 = this.GetDistanceTo(entity1);
		float distTo2 = this.GetDistanceTo(entity2);
		if(distTo1 >= distTo2) {
			return entity1;
		} else {
			return entity2;
		}
	}

	public T GetClosest<T>(List<T> entities) where T : EntitySpazioTemporale {
		return GetSorted(entities).FirstOrDefault();
	}

	public T GetClosest<T>(Game game) where T : EntitySpazioTemporale {
		return GetClosest<T>(game.entities.Where(e => e is T && e != this).Cast<T>().ToList());
	}

	public T GetFurthest<T>(List<T> entities) where T : EntitySpazioTemporale {
		return GetSorted(entities).LastOrDefault();
	}

	public T GetFurthest<T>(Game game) where T : EntitySpazioTemporale {
		return GetFurthest<T>(game.entities.Where(e => e is T).Cast<T>().ToList());
	}

	public override string ToString() {
		return base.ToString()+" "+pos;
	}
}



public class Ship : EntitySpazioTemporale {
	public int orientation;
	public int speed;
	public int stock;

	public bool canShoot;

	public Ship(int entityId, int x, int y, int orientation, int speed, int stock, int owner) :	base(entityId, x, y) {
		this.orientation = orientation;
		this.speed = speed;
		this.stock = stock;
		this.owner = (Owner) owner;			
	}

	public Position GetPunta() {
		//TODO
		return null;
	}

	public Position GetCulo() {
		//TODO
		return null;
	}

	public Ship GetClosestEnemy(Game game) {
		return base.GetSorted<Ship>(game.ships).Where(s => s.owner == Owner.Enemy).First();
	}

	public Ship GetClosestAlly(Game game) {
		return base.GetSorted<Ship>(game.ships).Where(s => s.owner == Owner.Player).First();
	}
}

public class Barrel : EntitySpazioTemporale {
	public int rum;

	public Barrel(int entityId, int x, int y, int rum) : base(entityId, x, y) {
		this.rum = rum;			
	}
}
	 
public class Mine : EntitySpazioTemporale {
	public static int MineDamage = 25;

	public Mine(int entityId, int x, int y) : base(entityId, x, y) {

	}
}

public class CannonSplash : Singularity {
	public Position target;
	public Ship shooter;
	public int eta;

	public CannonSplash(Ship shootingShip, int eta, int entityId, int x, int y) : base(entityId) {
		this.shooter = shootingShip;
		this.eta = eta;
		this.target = new Position(x, y);
	}
}
#endregion