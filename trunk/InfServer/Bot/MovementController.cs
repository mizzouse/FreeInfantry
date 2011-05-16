﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InfServer.Game;
using InfServer.Protocol;

using Assets;

namespace InfServer.Bots
{
	// MovementController Class
	/// Maintains the bot's object state based on simple movement instructions
	///////////////////////////////////////////////////////
    public class MovementController
	{	// Member variables
		///////////////////////////////////////////////////
		private VehInfo.Car _type;						//The type of vehicle we represent
		private Helpers.ObjectState _state;				//Our internal object state
        private Arena _arena;                           //The arena we're in

		//Hardcode the terrain for now until we get access to the map info!
		private int _terrainType = 0;
		
		//Our internal representation of position/velocity/direction
		//These are vectors using float precision
		private Vector2 _position;
		private Vector2 _velocity;
		private double _direction;
		private double _thrustDirection;

		//Our vehicle movement statistics, we use these as they may be
		//changed due to various things in the game world
		private int _rollTopSpeed;
		private int _rollThrust;
		private int _rollThrustStrafe;
		private int _rollThrustBack;
		private double _rollFriction;
		private int _rollRotate;

		//Internals which define what the bot should do each update
		private bool _isThrustingForward = false;
		private bool _isThrustingBackward = false;
		private bool _isStrafingLeft = false;
		private bool _isStrafingRight = false;
		private bool _isRotatingLeft = false;
		private bool _isRotatingRight = false;


		///////////////////////////////////////////////////
		// Member Functions
		///////////////////////////////////////////////////
		/// <summary>
		/// Generic Constructor
		/// </summary>
        public MovementController(VehInfo.Car type, Helpers.ObjectState state, Arena arena)
        {
			_type = type;
			_state = state;
            _arena = arena;

			_position = new Vector2(state.positionX, state.positionY);
			_velocity = new Vector2(state.velocityX, state.velocityY);
			_direction = state.yaw;

			//Copy over the relevant vehicle values
			SpeedValues t0 = type.TerrainSpeeds[0];

			_rollFriction = t0.RollFriction;
			_rollThrust = t0.RollThrust;
			_rollThrustStrafe = t0.StrafeThrust;
			_rollThrustBack = t0.BackwardThrust;
			_rollRotate = t0.RollRotate;
			_rollTopSpeed = t0.RollTopSpeed;
        }

		/// <summary>
		/// Calculates the new angle direction based on a change
		/// </summary>
        public double calculateNewDirection(double direction, int change)
        {
            direction += change;
            direction %= 240;

            if (direction < 0)
                direction = 240 + direction;

            return direction;
        }

		/// <summary>
		/// Updates the state in accordance with movement instructions
		/// </summary>
        public bool updateState(double delta)
        {	//Get our speed values for our current terrain
            SpeedValues stats = _type.TerrainSpeeds[_terrainType];

			//Apply our rotation
            if (_isRotatingLeft)
				_direction -= ((_rollRotate / 10000.0d) * delta);
			else if (_isRotatingRight)
				_direction += ((_rollRotate / 10000.0d) * delta);

			_direction %= 240;

			if (_direction < 0)
				_direction = 240 + _direction;

			//Vectorize our direction, unf.
			Vector2 directionVector = Vector2.createUnitVector(_direction);

			//Apply our thrusting instructions
            _thrustDirection = 0;

			if (_isThrustingForward)
			{
				_velocity.x += _rollThrust * directionVector.x;
				_velocity.y += _rollThrust * directionVector.y;
				_thrustDirection = _direction;
			}
			else if (_isThrustingBackward)
			{
				_velocity.x -= _rollThrustBack * directionVector.x;
				_velocity.y -= _rollThrustBack * directionVector.y;
				_thrustDirection = calculateNewDirection(_direction, 120);
			}

			if (_isStrafingLeft)
			{
				_velocity.x += _rollThrustStrafe * directionVector.y;
				_velocity.y += _rollThrustStrafe * -directionVector.x;
			}
			else if (_isStrafingRight)
			{
				_velocity.x += _rollThrustStrafe * -directionVector.y;
				_velocity.y += _rollThrustStrafe * directionVector.x;
			}

			double xPerTick = _velocity.x / 10000.0d;
			double yPerTick = _velocity.y / 10000.0d;

            //TODO: Add the effects of projectile explosions on our velocity
            //TODO: Check for physics react accordingly

			//Clamp our resulting velocity
			if (_velocity.magnitude() >= _rollTopSpeed)
				_velocity.setMagnitude(_rollTopSpeed);   
 
            //Apply friction laws of the terrain/car
			//TODO: The constant may not be entirely accurate
			double friction = 1 - Math.Log10(1 + (_rollFriction * delta) / 10000);

			_velocity.x *= friction;
			_velocity.y *= friction;

			xPerTick = _velocity.x / 10000.0d;
			yPerTick = _velocity.y / 10000.0d;

			Vector2 newPosition = new Vector2(_position.x + (xPerTick * delta), _position.y + (yPerTick * delta));

			//Check for collisions
			int tileX = (int)Math.Floor(_position.x / 16);
			int tileY = (int)Math.Floor(_position.y / 16);
			int newTileX = (int)Math.Floor(newPosition.x / 16);
			int newTileY = (int)Math.Floor(newPosition.y / 16);
			LvlInfo.Tile tile = _arena._tiles[(newTileY * _arena._levelWidth) + newTileX];
			if (tile.Blocked)
			{
                bool collision = false;
                if (Math.Abs(tileX - newTileX) > 0)
                {
                    _velocity.x *= -1;
                    collision = true;
                }
                if (Math.Abs(tileY - newTileY) > 0)
                {
                    _velocity.y *= -1;
                    collision = true;
                }
                if (collision)
                    _velocity.multiply(_type.BouncePercent / 1000.0d);
                
			}

            //Finally, we can adjust our position
            _position.x += xPerTick * delta;
			_position.y += yPerTick * delta;

			//Clamp our position
			_position.x = Math.Max(_position.x, 0);
			_position.y = Math.Max(_position.y, 0);

            //Update the state, converting our floats into the nearest short values
			_state.positionX = (short)_position.x;
			_state.positionY = (short)_position.y;
			_state.velocityX = (short)_velocity.x;
			_state.velocityY = (short)_velocity.y;
			_state.yaw = (byte)_direction;
			_state.direction = getDirection();

			//TODO: Apply vector tolerance to decide whether to update
			return true;
		}

		#region Movement Controls
		/// <summary>
		/// Causes the bot to thrust forward
		/// </summary>
		public void thrustForward()
        {
            _isThrustingForward = true;
			_isThrustingBackward = false;
        }

		/// <summary>
		/// Causes the bot to thrust backwards
		/// </summary>
        public void thrustBackward()
        {
			_isThrustingForward = false;
            _isThrustingBackward = true;
        }

		/// <summary>
		/// Causes the bot to strafe left
		/// </summary>
        public void strafeLeft()
        {
            _isStrafingLeft = true;
			_isStrafingRight = false;
        }

		/// <summary>
		/// Causes the bot to strafe right
		/// </summary>
        public void strafeRight()
        {
			_isStrafingLeft = false;
            _isStrafingRight = true;
        }

		/// <summary>
		/// Stops all thrust movement
		/// </summary>
        public void stopThrusting()
        {
            _isThrustingForward = false;
            _isThrustingBackward = false;
        }

		/// <summary>
		/// Stops all strafe movement
		/// </summary>
        public void stopStrafing()
        {
            _isStrafingLeft = false;
            _isStrafingRight = false;
        }

		/// <summary>
		/// Causes the bot to rotate left
		/// </summary>
        public void rotateLeft()
        {
            _isRotatingLeft = true;
			_isRotatingRight = false;
        }

		/// <summary>
		/// Causes the bot to rotate right
		/// </summary>
        public void rotateRight()
        {
			_isRotatingLeft = false;
            _isRotatingRight = true;
        }

		/// <summary>
		/// Stops all rotational movement
		/// </summary>
        public void stopRotating()
        {
            _isRotatingLeft = false;
            _isRotatingRight = false;
        }

		/// <summary>
		/// Stops all movement completely
		/// </summary>
		/// <remarks>May take a while for the bot to come to a stop.</remarks>
        public void stop()
        {
            stopThrusting();
            stopStrafing();
            stopRotating();
		}
		#endregion

		#region Utility functions
		/// <summary>
		/// Determines the hardcoded direction value from our movement instructions
		/// </summary>
		public Helpers.ObjectState.Direction getDirection()
		{
			if (_isThrustingForward)
			{
				if (_isStrafingLeft)
					return Helpers.ObjectState.Direction.NorthWest;
				else if (_isStrafingRight)
					return Helpers.ObjectState.Direction.NorthEast;
				else
					return Helpers.ObjectState.Direction.Forward;
			}
			else if (_isThrustingBackward)
			{
				if (_isStrafingLeft)
					return Helpers.ObjectState.Direction.SouthWest;
				else if (_isStrafingRight)
					return Helpers.ObjectState.Direction.SouthEast;
				else
					return Helpers.ObjectState.Direction.Backward;
			}
			else if (_isStrafingLeft)
				return Helpers.ObjectState.Direction.StrafeLeft;
			else if (_isStrafingRight)
				return Helpers.ObjectState.Direction.StrafeRight;

			return Helpers.ObjectState.Direction.None;
		}
		#endregion
	}
}