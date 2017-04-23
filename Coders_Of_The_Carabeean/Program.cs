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
		Game game = new Game();

		// game loop
		while (true) {
			// INIZIO TURNO
			List<Ship> ships = new List<Ship>();
			List<Barrel> barrels = new List<Barrel>();
			List<Mine> mines = new List<Mine>();
			List<CannonSplash> cannons = new List<CannonSplash>();
			

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
						Console.Error.WriteLine("Cannonata con target " + cannon.target + " arriverà tra " + cannon.eta);
						break;
				}
			}
			//------------------------------------------------
			game.UpdateInfo(ships, barrels, mines, cannons, turn);
			string myMove = barbanera.Think(game);
			Console.WriteLine(myMove);
			turn++;
		}
	}
}

#region Data Classes

public class Barbanera {

	Position[] preImpostate= new Position[] {new Position(3,3), new Position(11,2) , new Position(19,4)  , new Position(19,9) , new Position(20,16)  , new Position(6,17) };
	bool arrived=false;
	int j=-1;

	int step = 0;
	int ascissa = 2;

	public string Think(Game game) {
		string result = "";
													
		List<Barrel> barrels = game.barrels;

		List<Ship> myShips = game.GetAlliedShips();

		for (int i = 0; i < myShips.Count; i++) {

			Ship currentShip = myShips[i];
			if (currentShip.canShoot==false ) {
				Console.Error.WriteLine("BOOST");
				result += "FASTER\n";
				continue;
			}

			Ship closestEnemy = currentShip.GetClosestEnemy(game);
			Barrel closestBarrel = currentShip.GetClosest<Barrel>(barrels);

			if (game.barrels.Count == 0) { //Late Game
				if (j == -1) {
					j = preImpostate.ToList().IndexOf( currentShip.GetClosest(preImpostate.ToList()) );

				}
				if(currentShip.pos.Equals(preImpostate[j])) {
					arrived = true;
				} else {
					arrived = false;
				}

				if (arrived==true) {
					j = (j + 1) % 6;
				}

				if (currentShip.Prua.GetDistance(closestEnemy.pos) <= 3 && currentShip.canShoot) { //Ship close to Enemy 
					Position target = ComputeShotTarget(currentShip, closestEnemy);
					var str = currentShip.ShootAt(target);
					if (str == "dummy") {
						str = "FIRE " + closestEnemy.pos.x + " " + closestEnemy.pos.y+"\n";
					}
					result += str;	   
				} else {
					Console.Error.WriteLine("Arrived "+arrived+"position "+currentShip.pos);
					result += ("MOVE " + preImpostate[j].x + " " + preImpostate[j].y + "\n");
				}
				
				

			} else if (currentShip.GetDistanceTo(closestEnemy) <= 5 && currentShip.canShoot) { //Ship close to Enemy 

				result += ("FIRE " + closestEnemy.pos.x + " " + closestEnemy.pos.y + "\n");
				currentShip.hasShotLastTurn = true;
			} else {
				Position inFront = currentShip.Prua + currentShip.GetDirection();
				if ( game.mines.FirstOrDefault(mine => mine.pos.Equals(inFront)) != null){
					result += ("");
				}
				result += ("MOVE " + closestBarrel.pos.x + " " + closestBarrel.pos.y + "\n");
				barrels.Remove(closestBarrel);
			}
		}

		//Print Actions
		return result.Substring(0, result.Length-1);
	}

	public Position ComputeShotTarget(Ship myShip, Ship enemyShip) {
		if (myShip.Prua.GetDistance(enemyShip.pos) > 3 ) {
			throw new Exception("Distance is "+ myShip.Prua.GetDistance(enemyShip.pos)+", should be 3");
		}

		switch (enemyShip.speed) {
			case 0:
				return enemyShip.pos;
			case 1:
				switch (myShip.Prua.GetDistance(enemyShip.pos)) {
					case 0:
						return enemyShip.pos + enemyShip.GetDirection();
					case 1:
						return enemyShip.pos + (enemyShip.GetDirection() * 2);
					case 2:
						return enemyShip.pos + (enemyShip.GetDirection() * 2);
					case 3:
						return enemyShip.pos + (enemyShip.GetDirection() * 3);
				}
				break;
			case 2:
				switch (myShip.Prua.GetDistance(enemyShip.pos)) {
					case 0:
						return enemyShip.pos + (enemyShip.GetDirection()*2);
					case 1:
						return enemyShip.pos + (enemyShip.GetDirection() * 6);
					case 2:
						return enemyShip.pos + (enemyShip.GetDirection() * 8);
					case 3:
						return enemyShip.pos + (enemyShip.GetDirection() * 10);
				}
				break;
		}
		throw new Exception("Shouldn't be here");
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
	public int turn;

	public List<EntitySpazioTemporale> entities {
		get {
			return ships.Cast<EntitySpazioTemporale>()
				.Concat(barrels.Cast<EntitySpazioTemporale>())
				.Concat(mines.Cast<EntitySpazioTemporale>())
				.Concat(cannons.Cast<EntitySpazioTemporale>())
				.ToList();
		}
	}

	public Game() {

	}

	public Game(List<Ship> ships, List<Barrel> barrels, List<Mine> mines, List<CannonSplash> cannons, int turns) {
		this.ships = ships;
		this.barrels = barrels;
		this.mines = mines;
		this.cannons = cannons;
		this.turn = turns;
	}

	public void UpdateInfo(List<Ship> newShips, List<Barrel> newBarrels, List<Mine> newMines, List<CannonSplash> newCannons, int currentTurn) {
		if (currentTurn==0) {
			this.ships = newShips;
		} else {
			ships = ships.Where(s => newShips.Select(s1 => s1.entityId).Contains(s.entityId)).ToList(); 			

			newShips.ForEach(ship => {
				var otherShip = ships.FirstOrDefault(s => s.entityId == ship.entityId);
				if (otherShip != null) {
					ship.canShoot = otherShip.hasShotLastTurn == false;
				}
			});
		}

		this.ships = newShips;		
		this.barrels = newBarrels;
		this.mines = newMines;
		this.cannons = newCannons;
		this.turn = currentTurn;
	}

	public List<Ship> GetShips(int owner) {
		var result = from ship in ships
					 where ship.owner == (Owner) owner
					 select ship;

		return result.ToList();
	}

	public List<Ship> GetAlliedShips() {
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

	public Position(CubicPosition pos) {
		this.x = x + (pos.z - (pos.z & 1)) / 2;
		this.y = pos.z;
	}

	public override string ToString() {
		return string.Format("({0}, {1})", x, y);
	}

	public override bool Equals(object obj) {
		Position other = (Position) obj;
		return this.x == other.x && this.y == other.y;
	}

	public static Position operator *(Position pos, int mult) {
		return new Position(pos.x*mult, pos.y*mult);
	}

	public static Position operator +(Position pos1, Position pos2) {
		return new Position(pos1.x + pos2.x, pos1.y + pos2.y);
	}
	
	public int GetDistance(Position otherPosition) {
		return new CubicPosition(this).GetDistance(new CubicPosition(otherPosition));
	}
}

public class CubicPosition {
	public int x, y, z;

	public CubicPosition(Position offsetCoordinates) {
		var col = offsetCoordinates.x;
		var row = offsetCoordinates.y;
		x = col - (row - (row & 1)) / 2;
		z = row;
		y = -x - z;

		//if (offsetCoordinates.y % 2 == 0) {
		//	x = col;
		//	z = row - (col + (col & 1)) / 2;
		//	y = -x - z;
		//} else{
		//	x = col;
		//	z = row - (col - (col & 1)) / 2;
		//	y = -x - z;
		//}
	}

	public int GetDistance(CubicPosition otherPos) {
		return (Math.Abs(x - otherPos.x) + Math.Abs(y - otherPos.y) + Math.Abs(z - otherPos.z)) / 2;
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

	public float GetDistanceTo(Position pos) {
		return Math.Abs(this.pos.x - pos.x) + Math.Abs(this.pos.y - pos.y);
	}

	public float GetDistanceTo(EntitySpazioTemporale entity) {
		return GetDistanceTo(entity.pos);
	}

	public List<T> GetSorted<T>(List<T> entities) where T : EntitySpazioTemporale {
		List<T> result = new List<T>(entities);
		result.Sort(
			(T e1, T e2) => {
			return this.GetDistanceTo(e1).CompareTo(this.GetDistanceTo(e2));
		});
		return result;
	}

	public List<Position> GetSorted(List<Position> positions) {

		List<Position> result = new List<Position>(positions);
		result.Sort(
			(Position p1, Position p2) => {
				return this.GetDistanceTo(p1).CompareTo(this.GetDistanceTo(p2));
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

	public Position GetClosest(List<Position> position) {
		return GetSorted(position).FirstOrDefault();
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

	public static Position[] evenRowDirections = new Position[]
		{ new Position(1,0), new Position(0,-1), new Position(-1,-1),
		  new Position(-1,0), new Position(-1,1), new Position(0 , 1) };
	public static Position[] oddRowDirections = new Position[]
		{ new Position(1,0), new Position(1,-1), new Position(0,-1),
		  new Position(-1,0), new Position(0,1), new Position(1 , 1) };
	
	public int orientation;
	public int speed;
	public int stock;


	public Position Prua {
		get {
			return this.pos + GetDirection();
		}
	}

	public Position Poppa {
		get {
			return this.pos + GetDirection((orientation + 3) % 6);
		}
	}

	public bool canShoot;
	public bool hasShotLastTurn = false;

	public Ship(int entityId, int x, int y, int orientation, int speed, int stock, int owner) :	base(entityId, x, y) {
		this.orientation = orientation;
		this.speed = speed;
		this.stock = stock;
		this.owner = (Owner) owner;			
	}

	//public Position GetPrua() {
	//	return this.pos + GetDirection();
	//}

	//public Position GetPoppa() {
	//	return this.pos + GetDirection((orientation+3)%6);
	//}

	public Ship GetClosestEnemy(Game game) {
		return base.GetSorted<Ship>(game.GetOpponentShips()).First();
	}

	public Ship GetClosestAlly(Game game) {
		return base.GetSorted<Ship>(game.GetAlliedShips()).First();
	}

	public string ShootAt(Position target) {
		if(target.x < 0 || target.x >= 23 || target.y <0 || target.y >= 21) {
			return "dummy";
		}
		if(canShoot == false) {
			throw new Exception("Guarda che hai sbagliato. Non posso sparare");
		}
		hasShotLastTurn = true;
		return "FIRE " + target.x + " " + target.y+"\n";		
	}

	public Position GetDirection() {
		if(pos.y%2 == 0) {
			return evenRowDirections[orientation];
		} else {
			return oddRowDirections[orientation];
		}
	}

	public Position GetDirection(int orientation) {
		if (pos.y % 2 == 0) {
			return evenRowDirections[orientation];
		} else {
			return oddRowDirections[orientation];
		}
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