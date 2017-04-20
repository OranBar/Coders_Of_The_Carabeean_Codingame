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

		int movx = 0;
		int movy = 0;
		int posx = 0;
		int posy = 0;
		float closest= int.MaxValue;
		// game loop
		while (true) {
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
				if (entityType == "SHIP" && arg4==1 ){
					posx = x;
					posy = y;
				}
				if (entityType == "BARREL" && GetDistance(x,y,posx,posy)<closest){
					movx = x;
					movy = y;
					closest = GetDistance(x, y, posx, posy);
				}
			}
			//------------------------------------------------


			// Write an action using Console.WriteLine()
			// To debug: Console.Error.WriteLine("Debug messages...");

			Console.WriteLine("MOVE " + movx + " " + movy); // Any valid action, such as "WAIT" or "MOVE x y"
			closest = int.MaxValue;
				
		}
	}

	public static float GetDistance(int x1, int y1, int x2, int y2) {
		return Math.Abs(x2-x1)+ Math.Abs(y2-y1);
	}
}