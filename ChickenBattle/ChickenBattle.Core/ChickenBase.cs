using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ChickenBattle.Core
{
    public abstract class ChickenBase : NotifyPropertyBase, IChicken
    {
        private int _x;
        private int _y;
        private int _angle;
        private readonly GameInfo _gameInfo;
        private readonly Dictionary<int, Point> _directionMapping; 
        private int _hitPoints;
        private bool _canMove;
        private bool _canRotate;

        protected Dictionary<int, Point> DirectionMapping
        {
            get { return _directionMapping; }
        }

        protected GameInfo GameInfo
        {
            get { return _gameInfo; }
        }
        
        protected Point GetTargetCoord()
        {
            return new Point(_x + DirectionMapping[_angle].X, _y + DirectionMapping[_angle].Y);
        }

        protected virtual void MoveLeft()
        {
            if (_canMove && _x > 0 && GameInfo.Players.FirstOrDefault(p => p.X == _x - 1 && p.Y == _y) == null)
            {
                _canMove = false;
                _x--;
                OnPropertyChanged("X");
            }
        }

        protected virtual void MoveRight()
        {
            if (_canMove && _x < GameInfo.FieldWidth && GameInfo.Players.FirstOrDefault(p => p.X == _x + 1 && p.Y == _y) == null)
            {
                _canMove = false;
                _x++;
                OnPropertyChanged("X");
            }
        }

        protected virtual void MoveUp()
        {
            if (_canMove && _y > 0 && GameInfo.Players.FirstOrDefault(p => p.X == _x && p.Y == _y - 1) == null)
            {
                _canMove = false;
                _y--;
                OnPropertyChanged("Y");
            }
        }

        protected virtual void MoveDown()
        {
            if (_canMove && _y < GameInfo.FieldHeight && GameInfo.Players.FirstOrDefault(p => p.X == _x && p.Y == _y + 1) == null)
            {
                _canMove = false;
                _y++;
                OnPropertyChanged("Y");
            }
        }

        protected virtual void RotateClockwise()
        {
            if (_canRotate)
            {
                _canRotate = false;
                _angle = (_angle + 45)%360;
                OnPropertyChanged("Angle");
            }
        }

        protected virtual void InflictDamage()
        {
            if (_canRotate)
            {
                _canRotate = false;
                
                Point targetCoord = GetTargetCoord();
                IChicken target =
                    GameInfo.Players.FirstOrDefault(
                        t => (int) targetCoord.X == t.X && (int) targetCoord.Y == t.Y);

                if (target != null)
                {
                    target.HitPoints--;
                }
            }
        }

        protected virtual void RotateCounterclockwise()
        {
            if (_canMove)
            {
                _canMove = false;
                _angle -= _angle - 45;
                if (_angle < 0)
                {
                    _angle += 360;
                }
                _angle %= 360;

                OnPropertyChanged("Angle");
            }
        }

        protected ChickenBase(GameInfo gameInfo)
        {
            if (gameInfo == null)
                throw new ArgumentNullException("gameInfo");

            _gameInfo = gameInfo;
            _hitPoints = _gameInfo.ChickenHitPoints;

            _directionMapping = new Dictionary<int, Point>
                                                          {
                                                              { 0, new Point(1, 0) },
                                                              { 45, new Point(1, 1) },
                                                              { 90, new Point(0, 1) },
                                                              { 135, new Point(-1, 1) },
                                                              { 180, new Point(-1, 0) },
                                                              { 225, new Point(-1, -1) },
                                                              { 270, new Point(0, -1) },
                                                              { 315, new Point(1, -1) }
                                                          };

            Random r = new Random();
            do
            {
                _x = r.Next(GameInfo.FieldWidth);
                _y = r.Next(GameInfo.FieldHeight);
            } while (GameInfo.Players.FirstOrDefault(p => p.X == _x && p.Y == _y) != null);

            _angle = r.Next(8)*45;
        }

        public void MakeTurn()
        {
            _canMove = true;
            _canRotate = true;
            OnMakeTurn();
        }

        protected abstract void OnMakeTurn();

        public int Angle
        {
            get { return _angle; }
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public int HitPoints
        {
            get { return _hitPoints; }
            set 
            { 
                _hitPoints = value;
                OnPropertyChanged("HitPoints");
            }
        }

        public abstract string AiName { get; }
    }
}
