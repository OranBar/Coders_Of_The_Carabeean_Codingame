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
			Ship closestEnemy = null;
			float minShipDistance = float.MaxValue;
			foreach (Ship enemyShip in game.GetOpponentShips()) {
				float distance = currentShip.GetDistanceTo(enemyShip);
				//float distance = GetDistance(currentShip.pos.x, currentShip.pos.y, enemyShip.pos.x, enemyShip.pos.y);
				if (distance < minShipDistance) {
					closestEnemy = enemyShip;
					minShipDistance = distance;
				}
			}
			//--------------------------------

			//Find closest barrel
			Barrel closestBarrel = null;
			float minBarrelDistance = float.MaxValue;
			foreach (Barrel barrel in game.barrels) {
				//float distance = currentShip.GetDistanceTo(enemyShip);
				float distance = currentShip.GetDistanceTo(barrel);
				//float distance = GetDistance(currentShip.pos.x, currentShip.pos.y, barrel.pos.x, barrel.pos.y);
				if (distance < minBarrelDistance) {
					closestBarrel = barrel;
					minBarrelDistance = distance;
				}
			}
			//-----------------


			if (game.barrels.Count == 0) {
				if (game.turns % 2 == 0) {
					Console.Error.WriteLine("No more barrels");
					int casx = new Random().Next(0, 5);
					int casy = new Random().Next(0, 5);
					//Console.WriteLine("MOVE " + casx + " " + casy);
					result += "MOVE " + casx + " " + casy+"\n";
				} else {
					//Console.WriteLine("FIRE " + closestEnemy.pos.x + " " + closestEnemy.pos.y);
					result += "FIRE " + closestEnemy.pos.x + " " + closestEnemy.pos.y + "\n";
				}

			} else if (currentShip.GetDistanceTo(closestEnemy) <= 5) {
				//Console.WriteLine("FIRE " + closestEnemy.pos.x + " " + closestEnemy.pos.y);
				result += ("FIRE " + closestEnemy.pos.x + " " + closestEnemy.pos.y + "\n");
			} else {
				//Console.WriteLine("MOVE " + closestBarrel.pos.x + " " + closestBarrel.pos.y);
				result += ("MOVE " + closestBarrel.pos.x + " " + closestBarrel.pos.y + "\n");
			}
		}
		return result.Substring(0, result.Length-1);
	}


}

public class Game {

	public List<Ship> ships = new List<Ship>();
	public List<Barrel> barrels = new List<Barrel>();
	public List<Mine> mines = new List<Mine>();
	public List<CannonSplash> cannons = new List<CannonSplash>();
	public int turns;

	public Game(List<Ship> ships, List<Barrel> barrels, List<Mine> mines, List<CannonSplash> cannons, int turns) {
		this.ships = ships;
		this.barrels = barrels;
		this.mines = mines;
		this.cannons = cannons;
		this.turns = turns;
	}

	public List<Ship> GetShips(int owner) {
		var result = from ship in ships
					 where ship.owner == owner
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
}

public abstract class Singularity {
	public int entityId;

	public Singularity(int entityId) {
		this.entityId = entityId;
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

}



public class Ship : EntitySpazioTemporale {
	public int orientation;
	public int speed;
	public int stock;
	public int owner;

	public bool canShoot;

	public Ship(int entityId, int x, int y, int orientation, int speed, int stock, int owner) :	base(entityId, x, y) {
		this.orientation = orientation;
		this.speed = speed;
		this.stock = stock;
		this.owner = owner;			
	}

	public Position GetPunta() {
		//TODO
		return null;
	}

	public Position GetCulo() {
		//TODO
		return null;
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