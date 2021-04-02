using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

//Author: Isaiah Weaver
//Description: This class controls everything about the movement of the chess game.
//NOTE: YOU MUST BE CONNECTED TO THE INTERNET FOR THE CHESS PIECE IMAGES TO LOAD!

namespace WeaverChess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int stage; // 0: game hasn't started, 1: white's turn, 3: black's turn, 2: white move piece, 4: black move piece, 6: game over.
        Point[] globalSpots;
        ChessPiece globalPiece;
        private TimeSpan WhiteMinutes;
        private TimeSpan BlackMinutes;
        private DispatcherTimer t;
        private bool checkIsChecked;
        public MainWindow()
        {
            InitializeComponent();
            stage = 0;
            RbtnWhiteTurn.IsHitTestVisible = false;
            RbtnBlackTurn.IsHitTestVisible = false;
            WhiteMinutes = new TimeSpan(0, 0, 10, 0, 0);
            BlackMinutes = new TimeSpan(0, 0, 10, 0, 0);
            checkIsChecked = false;
            this.ChessBoard.ItemsSource = new ObservableCollection<ChessPiece>
            {
                new ChessPiece{Pos=new Point(0, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(100, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(200, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(300, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(400, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(500, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(600, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(700, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(0, 700), Type=PieceType.Rook, Player=Player.White},
                new ChessPiece{Pos=new Point(100, 700), Type=PieceType.Knight, Player=Player.White},
                new ChessPiece{Pos=new Point(200, 700), Type=PieceType.Bishop, Player=Player.White},
                new ChessPiece{Pos=new Point(400, 700), Type=PieceType.King, Player=Player.White},
                new ChessPiece{Pos=new Point(300, 700), Type=PieceType.Queen, Player=Player.White},
                new ChessPiece{Pos=new Point(500, 700), Type=PieceType.Bishop, Player=Player.White},
                new ChessPiece{Pos=new Point(600, 700), Type=PieceType.Knight, Player=Player.White},
                new ChessPiece{Pos=new Point(700, 700), Type=PieceType.Rook, Player=Player.White},
                new ChessPiece{Pos=new Point(0, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(100, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(200, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(300, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(400, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(500, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(600, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(700, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(0, 0), Type=PieceType.Rook, Player=Player.Black},
                new ChessPiece{Pos=new Point(100, 0), Type=PieceType.Knight, Player=Player.Black},
                new ChessPiece{Pos=new Point(200, 0), Type=PieceType.Bishop, Player=Player.Black},
                new ChessPiece{Pos=new Point(400, 0), Type=PieceType.King, Player=Player.Black},
                new ChessPiece{Pos=new Point(300, 0), Type=PieceType.Queen, Player=Player.Black},
                new ChessPiece{Pos=new Point(500, 0), Type=PieceType.Bishop, Player=Player.Black},
                new ChessPiece{Pos=new Point(600, 0), Type=PieceType.Knight, Player=Player.Black},
                new ChessPiece{Pos=new Point(700, 0), Type=PieceType.Rook, Player=Player.Black},
            };
        }

        private void PicChessPiece_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (stage != 1 && stage != 3)
                return;

            Point p = e.GetPosition(this);
            p.X = ((int)(p.X / 100)) * 100;
            p.Y = ((int)(p.Y / 100)) * 100;

            ChessPiece cur = GetPieceByPoint(p);

            //HANDLES THE TURNS: stop if the clicked piece is the wrong team
            if (cur.Player == Player.Black && stage == 1 )
                return;
            if (cur.Player == Player.White && stage == 3)
                return;

            //HIGHLIGHT THE PIECE AND THE PLACES IT CAN MOVE
            Point[] spots = GetMoveSpots(cur);
            spots = RemoveNonCheckPoints(spots, cur);
            spots = AddSamePoint(spots, cur);
            //SKIP IF THE KING CAN'T MOVE ANYWHERE
            if (spots.Length == 0)
                return;
            InsertCircles(spots);

            globalSpots = spots;
            globalPiece = cur;
            NextStage();
        }

        private void CanvasDots_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (stage != 2 && stage != 4) // ADD HERE IF YOU ARE IN CHECK YOU HAVE TO MOVE YOUR KING
                return;
    
            Point p = e.GetPosition(this);
            p.X = ((int)(p.X / 100)) * 100;
            p.Y = ((int)(p.Y / 100)) * 100;

            //check if the clicked location is a red dot location
            foreach (Point i in globalSpots)
            {
                if (p.Equals(i))
                {
                    if (p == globalPiece.Pos) //if the clicked dot is the same as the piece
                    {
                        CanvasDots.Children.Clear();
                        PrevStage();
                        return;
                    }
                    if (GetPieceByPoint(p) != null) //HANDLES TAKING ANOTHER PIECE
                    {
                        ChessPiece del = GetPieceByPoint(p);
                        del.Pos = new Point(-200, 0);
                    }
                    //ADD ROOK MOVE IF CASTLING HERE
                    if (Math.Abs(globalPiece.Pos.X - p.X) > 100 && globalPiece.Type == PieceType.King)
                    {
                        //MOVE THE CORRECT ROOK TO INTS NEW POSITION
                        if (p.X == 200 && p.Y == 0)
                        {
                            ChessPiece rook = GetPieceByPoint(new Point(0, 0));
                            rook.Pos = new Point(300, 0);
                        }
                        if (p.X == 600 && p.Y == 0)
                        {
                            ChessPiece rook = GetPieceByPoint(new Point(700, 0));
                            rook.Pos = new Point(500, 0);
                        }
                        if (p.X == 200 && p.Y == 700)
                        {
                            ChessPiece rook = GetPieceByPoint(new Point(0, 700));
                            rook.Pos = new Point(300, 700);
                        }
                        if (p.X == 600 && p.Y == 700)
                        {
                            ChessPiece rook = GetPieceByPoint(new Point(700, 700));
                            rook.Pos = new Point(500, 700);
                        }
                    }
                    globalPiece.Pos = new Point(p.X, p.Y);

                    CheckPawnPromotion(globalPiece);
                    
                    if (IsInCheckmate(globalPiece.Player))
                    {
                        EndGame();
                        CanvasDots.Children.Clear();
                        return;
                    }
                    if (IsInCheck(globalPiece.Player))
                        TellUserInCheck();
                }
            }
            CanvasDots.Children.Clear();
            NextStage();

            if (RbtnBlackTurn.IsChecked == false)
            {
                RbtnBlackTurn.IsChecked = true;
                RbtnWhiteTurn.IsChecked = false;
            }
            else
            {
                RbtnBlackTurn.IsChecked = false;
                RbtnWhiteTurn.IsChecked = true;
            }
        }

        //returns null if a ChessPiece is not at point p
        private ChessPiece GetPieceByPoint(Point p)
        {
            bool pieceFound = false;
            ChessPiece cur = null; //default piece
            for (int i = 0; i < this.ChessBoard.Items.Count; i++)
            {
                cur = (ChessPiece)this.ChessBoard.Items.GetItemAt(i);
                if (cur.Pos.X == p.X && cur.Pos.Y == p.Y)
                {
                    pieceFound = true;
                    break;
                }
            }
            if (pieceFound)
                return cur;
            else
                return null;
        }

        private void InsertCircles(Point[] p)
        {
            for (int i = 0; i < p.Length; i++)
            {
                Ellipse myEllipse = new Ellipse();
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                mySolidColorBrush.Color = Colors.Green;
                myEllipse.Fill = mySolidColorBrush;
                myEllipse.Opacity = .50;
                myEllipse.StrokeThickness = 2;
                myEllipse.Stroke = Brushes.Lime;

                myEllipse.Width = 50;
                myEllipse.Height = 50;
                Canvas.SetLeft(myEllipse, p[i].X + 25);
                Canvas.SetTop(myEllipse, p[i].Y + 25);

                CanvasDots.Children.Add(myEllipse);
            }
        }

        //GETS ALL THE POINTS A PIECE cur CAN MOVE TO
        public Point[] GetMoveSpots(ChessPiece cur)
        {
            List<Point> points = new List<Point>();

            if (cur.Type == PieceType.Pawn)
            {
                if (cur.Player == Player.White)
                {
                    if (cur.Pos.Y == 600 && GetPieceByPoint((new Point(cur.Pos.X, cur.Pos.Y - 200))) == null && GetPieceByPoint((new Point(cur.Pos.X, cur.Pos.Y - 100))) == null)
                        points.Add(new Point(cur.Pos.X, cur.Pos.Y - 200));
                    if (GetPieceByPoint((new Point(cur.Pos.X, cur.Pos.Y - 100))) == null)
                        points.Add(new Point(cur.Pos.X, cur.Pos.Y - 100));
                    if (GetPieceByPoint((new Point(cur.Pos.X + 100, cur.Pos.Y - 100))) != null)
                        points.Add(new Point(cur.Pos.X + 100, cur.Pos.Y - 100));
                    if (GetPieceByPoint((new Point(cur.Pos.X - 100, cur.Pos.Y - 100))) != null)
                        points.Add(new Point(cur.Pos.X - 100, cur.Pos.Y - 100));
                }
                if (cur.Player == Player.Black)
                {
                    if (cur.Pos.Y == 100 && GetPieceByPoint((new Point(cur.Pos.X, cur.Pos.Y + 200))) == null && GetPieceByPoint((new Point(cur.Pos.X, cur.Pos.Y + 100))) == null)
                        points.Add(new Point(cur.Pos.X, cur.Pos.Y + 200));
                    if (GetPieceByPoint((new Point(cur.Pos.X, cur.Pos.Y + 100))) == null)
                        points.Add(new Point(cur.Pos.X, cur.Pos.Y + 100));
                    if (GetPieceByPoint((new Point(cur.Pos.X + 100, cur.Pos.Y + 100))) != null)
                        points.Add(new Point(cur.Pos.X + 100, cur.Pos.Y + 100));
                    if (GetPieceByPoint((new Point(cur.Pos.X - 100, cur.Pos.Y + 100))) != null)
                        points.Add(new Point(cur.Pos.X - 100, cur.Pos.Y + 100));
                }
            }
            else if (cur.Type == PieceType.Rook)
            {
                points.AddRange(GetRookPoints(cur));
            }
            else if (cur.Type == PieceType.Knight)
            {
                points.Add(new Point(cur.Pos.X - 200, cur.Pos.Y - 100));
                points.Add(new Point(cur.Pos.X - 100, cur.Pos.Y - 200));
                points.Add(new Point(cur.Pos.X + 100, cur.Pos.Y - 200));
                points.Add(new Point(cur.Pos.X + 200, cur.Pos.Y - 100));
                points.Add(new Point(cur.Pos.X + 200, cur.Pos.Y + 100));
                points.Add(new Point(cur.Pos.X + 100, cur.Pos.Y + 200));
                points.Add(new Point(cur.Pos.X - 200, cur.Pos.Y + 100));
                points.Add(new Point(cur.Pos.X - 100, cur.Pos.Y + 200));
            }
            else if (cur.Type == PieceType.Bishop)
            {
                points.AddRange(GetBishopPoints(cur));
            }
            else if (cur.Type == PieceType.Queen)
            {
                points.AddRange(GetBishopPoints(cur));
                points.AddRange(GetRookPoints(cur));
            }
            else if (cur.Type == PieceType.King)
            {
                points.AddRange(GetKingPoints(cur));
            }

            //REMOVE ALL THE INVALID POINTS AND FIXED KILLING YOUR OWN KING
            List<Point> newPoints = RemoveInvalidPoints(points, cur.Player);
            if (cur.Type != PieceType.King)
            {
                newPoints.Add(cur.Pos);
                for (int i = 0; i < this.ChessBoard.Items.Count; i++)
                {
                    ChessPiece temp = (ChessPiece)this.ChessBoard.Items.GetItemAt(i);
                    if (temp.Type == PieceType.King)
                        newPoints.Remove(new Point(temp.Pos.X, temp.Pos.Y));
                }
            }

            return newPoints.ToArray();
        }

        //REMOVE ALL OF THE POINTS THAT DON'T GET YOU OUT OF CHECK
        private Point[] RemoveNonCheckPoints(Point[] points, ChessPiece cur)
        {
            Player team;
            if (cur.Player == Player.White)
                team = Player.Black;
            else
                team = Player.White;

            List<Point> newPoints = new List<Point>();
            newPoints.AddRange(points);
            if (/*IsInCheck(team) && */cur.Player != team)
            {
                //newPoints.AddRange(points);
                foreach (Point p in points)
                {
                    Point oldPos = new Point(cur.Pos.X, cur.Pos.Y);
                    Point oldDelPos = new Point(p.X, p.Y);
                    ChessPiece delPiece = GetPieceByPoint(new Point(p.X, p.Y));
                    if (delPiece != null)
                        delPiece.Pos = new Point(-200, 0);
                    cur.Pos = p;
                    if (IsInCheck(team))
                        newPoints.Remove(p);
                    cur.Pos = oldPos;
                    if (delPiece != null)
                        delPiece.Pos = oldDelPos;
                }
                return newPoints.ToArray();
            }
            else if (cur.Player == team)
            {
                foreach (Point p in points)
                {
                    Point oldPos = new Point(cur.Pos.X, cur.Pos.Y);
                    Point oldDelPos = new Point(p.X, p.Y);
                    ChessPiece delPiece = GetPieceByPoint(new Point(p.X, p.Y));
                    if (delPiece != null)
                        delPiece.Pos = new Point(-200, 0);
                    cur.Pos = p;
                    if (IsInCheck(cur.Player))
                        newPoints.Remove(p);
                    cur.Pos = oldPos;
                    if (delPiece != null)
                        delPiece.Pos = oldDelPos;
                }
                return newPoints.ToArray();
            }
            return points;
        }

        //ADDS THE POINTS THAT THE cur CHESSPIECE IS AT TO THE Point[]
        private Point[] AddSamePoint(Point[] points, ChessPiece cur)
        {
            List<Point> newPoints = new List<Point>();
            newPoints.AddRange(points);
            newPoints.Add(new Point(cur.Pos.X, cur.Pos.Y));
            List<Point> distinct = newPoints.Distinct().ToList();
            return distinct.ToArray();
        }
        
        private bool IsInCheckmate(Player team)
        {
            for (int i = 0; i < this.ChessBoard.Items.Count; i++)
            {
                ChessPiece cur = (ChessPiece)this.ChessBoard.Items.GetItemAt(i);
                //THIS IF CHECK CHECKS IF THE KING CAN GET OUT OF CHECK
                if (cur.Type == PieceType.King && cur.Player != team)
                {
                    Point[] check = GetMoveSpots(cur);
                    if (check.Length > 1)
                        return false;
                }
            }

            //THIS CHECKS IF ANOTHER PIECE CAN GET THE KING OUT OF CHECK
            for (int j = 0; j < this.ChessBoard.Items.Count; j++)
            {
                ChessPiece cur2 = (ChessPiece)this.ChessBoard.Items.GetItemAt(j);
                if (cur2.Player != team && cur2.Type != PieceType.King && cur2.Pos.X >= 0)
                {
                    //GO THROUGH EACH POSSIBLE PLACE THE SELECTED PIECE CAN MOVE
                    Point[] tempPoints = GetMoveSpots(cur2);
                    for (int k = 0; k < tempPoints.Length; k++)
                    {
                        Point oldPos = new Point(cur2.Pos.X, cur2.Pos.Y);
                        Point oldDelPos = new Point(tempPoints[k].X, tempPoints[k].Y);
                        ChessPiece DelPiece = GetPieceByPoint(oldDelPos);
                        if (DelPiece != null)
                            DelPiece.Pos = new Point(-200, 0);
                        cur2.Pos = new Point(tempPoints[k].X, tempPoints[k].Y);
                        if (!IsInCheck(team))
                        {
                            cur2.Pos = oldPos;
                            if (DelPiece != null)
                                DelPiece.Pos = new Point(oldDelPos.X, oldDelPos.Y);
                            return false;
                        }
                        if (DelPiece != null)
                            DelPiece.Pos = new Point(oldDelPos.X, oldDelPos.Y);
                        cur2.Pos = oldPos;
                    }
                }
            }
            return true;
        }

        //CHECKS TO SEE IF THE TEAM OPPOSITE OF team IS IN CHECK
        private bool IsInCheck(Player team)
        {
            checkIsChecked = true;
            //get the opposite teams King's move points
            List<Point> check = null;
            ChessPiece cur = null;
            for (int i = 0; i < this.ChessBoard.Items.Count; i++)
            {
                cur = (ChessPiece)this.ChessBoard.Items.GetItemAt(i);
                if (cur.Type == PieceType.King && cur.Player != team)
                {
                    check = GetKingPoints(cur);
                    break;
                }
            }

            if (check == null)
                return true;
            if (!check.Contains(cur.Pos))
                return true;
            else
            {
                LblGameOver.Content = "";
                return false;
            }
        }

        //HELPER METHOD OF IsInCheck(Player)
        private void TellUserInCheck()
        {
            String check = "";
            if (stage == 1 || stage == 2)
                check += "Black";
            if (stage == 3 || stage == 4)
                check += "White";
            LblGameOver.Content = check + " is in check.\nSave your king.";
        }

        private List<Point> GetKingPoints(ChessPiece cur)
        {
            //ADD THE MAXIMUM POINTS THE KING COULD MOVE
            List<Point> points = new List<Point>();
            points.Add(new Point(cur.Pos.X, cur.Pos.Y));
            points.Add(new Point(cur.Pos.X - 100, cur.Pos.Y - 100));
            points.Add(new Point(cur.Pos.X - 100, cur.Pos.Y));
            points.Add(new Point(cur.Pos.X + 100, cur.Pos.Y - 100));
            points.Add(new Point(cur.Pos.X + 100, cur.Pos.Y));
            points.Add(new Point(cur.Pos.X + 100, cur.Pos.Y + 100));
            points.Add(new Point(cur.Pos.X, cur.Pos.Y + 100));
            points.Add(new Point(cur.Pos.X - 100, cur.Pos.Y + 100));
            points.Add(new Point(cur.Pos.X, cur.Pos.Y - 100));

            List<Point> newPoints = new List<Point>();
            List<Point> OtherPoints = new List<Point>();
            //FOR EACH NEW POSSIBLE POINT THE KING COULD MOVE
            for (int i = 0; i < points.Count; i++)
            {
                Point oldKilledPiecePos = new Point(points[i].X, points[i].Y);
                Point oldKingPos = new Point(cur.Pos.X, cur.Pos.Y);
                ChessPiece killedPiece = GetPieceByPoint(oldKilledPiecePos);
                cur.Pos = new Point(points[i].X, points[i].Y);
                if (killedPiece != null && killedPiece.Player != cur.Player)
                    killedPiece.Pos = new Point(-200, 0);

                //GO THROUGH ALL THE OTHER TEAM'S PIECES TO SEE IF THEY CAN KILL THE KING IN ITS NEW POSITION.
                for (int j = 0; j < this.ChessBoard.Items.Count; j++)
                {
                    ChessPiece check = (ChessPiece)this.ChessBoard.Items.GetItemAt(j);
                    //IF THE PIECE IS DEAD OR THE KING
                    if (check.Pos.X < 0 || check.Type == PieceType.King)
                        continue;
                    //ADD POINTS WHERE KING CAN'T GO BECAUSE IT WOULD PUT HIM IN HARM.
                    if (check.Player != cur.Player && check.Type != PieceType.Pawn)
                    {
                        OtherPoints.AddRange(GetMoveSpots(check));
                        OtherPoints.Remove(check.Pos);
                    }
                    //IF CHECK SO THE KING DOESN'T MOVE TO WHERE THE PAWNS CAN KILL IT
                    else if (check.Player != cur.Player && check.Type == PieceType.Pawn)
                    {
                        if (check.Player == Player.White)
                        {
                            OtherPoints.Add(new Point(check.Pos.X - 100, check.Pos.Y - 100));
                            OtherPoints.Add(new Point(check.Pos.X + 100, check.Pos.Y - 100));
                        }
                        if (check.Player == Player.Black)
                        {
                            OtherPoints.Add(new Point(check.Pos.X - 100, check.Pos.Y + 100));
                            OtherPoints.Add(new Point(check.Pos.X + 100, check.Pos.Y + 100));
                        }
                    }
                }
                //PUT THE PIECES BACK AFTER THE CALCULATION
                cur.Pos = oldKingPos;
                if (killedPiece != null)
                    killedPiece.Pos = oldKilledPiecePos;
            }
            //TAKE AWAY THE POINTS IN THE KING'S POINTS THAT WOULD PUT HIM IN HARM
            newPoints.AddRange(points);
            foreach (Point p in OtherPoints)
            {
                if (points.Contains(p))
                    newPoints.Remove(p);
            }

            //ADD THE CASTLING MOVE FOR THE KING
            if (checkIsChecked == true)
            {
                checkIsChecked = false;
                return newPoints;
            }

            Player tempTeam;
            if (cur.Player == Player.Black)
                tempTeam = Player.White;
            else
                tempTeam = Player.Black;

            if (!IsInCheck(tempTeam))
            {
                if (cur.Player == Player.White)
                {
                    if (cur.Pos.X == 400 && cur.Pos.Y == 700)
                    {
                        //GO THROUGH THE PIECES AND CHECK THE POSITIONS OF THE ROOKS THEN ADD THE CASTLING MOVE
                        for (int i = 0; i < this.ChessBoard.Items.Count; i++)
                        {
                            ChessPiece cur2 = (ChessPiece)this.ChessBoard.Items.GetItemAt(i);
                            if (cur2.Type == PieceType.Rook)
                            {
                                if (cur2.Pos.X == 0 && cur2.Pos.Y == 700 && GetPieceByPoint(new Point(100, 700)) == null && GetPieceByPoint(new Point(200, 700)) == null && GetPieceByPoint(new Point(300, 700)) == null)
                                    newPoints.Add(new Point(200, 700));
                                else if (cur2.Pos.X == 700 && cur2.Pos.Y == 700 && GetPieceByPoint(new Point(500, 700)) == null && GetPieceByPoint(new Point(600, 700)) == null)
                                    newPoints.Add(new Point(600, 700));
                            }
                        }
                    }
                }
                else if (cur.Player == Player.Black)
                {
                    if (cur.Pos.X == 400 && cur.Pos.Y == 0)
                    {
                        //GO THROUGH THE PIECES AND CHECK THE POSITIONS OF THE ROOKS THEN ADD THE CASTLING MOVE
                        for (int i = 0; i < this.ChessBoard.Items.Count; i++)
                        {
                            ChessPiece cur2 = (ChessPiece)this.ChessBoard.Items.GetItemAt(i);
                            if (cur2.Type == PieceType.Rook)
                            {
                                if (cur2.Pos.X == 0 && cur2.Pos.Y == 0 && GetPieceByPoint(new Point(100, 0)) == null && GetPieceByPoint(new Point(200, 0)) == null && GetPieceByPoint(new Point(300, 0)) == null)
                                    newPoints.Add(new Point(200, 0));
                                else if (cur2.Pos.X == 700 && cur2.Pos.Y == 0 && GetPieceByPoint(new Point(500, 0)) == null && GetPieceByPoint(new Point(600, 0)) == null)
                                    newPoints.Add(new Point(600, 0));
                            }
                        }
                    }
                }
            }

            checkIsChecked = false;
            return newPoints;
        }

        private void CheckPawnPromotion(ChessPiece cur)
        {
            if (cur.Type == PieceType.Pawn)
            {
                if (cur.Player == Player.White)
                {
                    if (cur.Pos.Y == 0)
                    {
                        PawnPromoMessage(cur);
                    }
                }
                if (cur.Player == Player.Black)
                {
                    if (cur.Pos.Y == 700)
                    {
                        PawnPromoMessage(cur);
                    }
                }
            }
        }

        private void PawnPromoMessage(ChessPiece cur)
        {
            MessageBoxResult result = MessageBox.Show("What piece would you like to promote your pawn to?\nYes: Queen\nNo: Knight", "Pawn Promotion", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    cur.Type = PieceType.Queen;
                    break;
                case MessageBoxResult.No:
                    cur.Type = PieceType.Knight;
                    break;
            }
        }

        private List<Point> GetBishopPoints(ChessPiece cur)
        {
            List<Point> points = new List<Point>();
            for (int i = 1; i < 8; i++)
            {
                Point p = new Point(cur.Pos.X - (i * 100), cur.Pos.Y - (i * 100));
                if (GetPieceByPoint(p) != null)
                {
                    points.Add(p);
                    break;
                }
                else
                    points.Add(p);
            }
            for (int i = 1; i < 8; i++)
            {
                Point p = new Point(cur.Pos.X + (i * 100), cur.Pos.Y - (i * 100));
                if (GetPieceByPoint(p) != null)
                {
                    points.Add(p);
                    break;
                }
                else
                    points.Add(p);
            }
            for (int i = 1; i < 8; i++)
            {
                Point p = new Point(cur.Pos.X - (i * 100), cur.Pos.Y + (i * 100));
                if (GetPieceByPoint(p) != null)
                {
                    points.Add(p);
                    break;
                }
                else
                    points.Add(p);
            }
            for (int i = 1; i < 8; i++)
            {
                Point p = new Point(cur.Pos.X + (i * 100), cur.Pos.Y + (i * 100));
                if (GetPieceByPoint(p) != null)
                {
                    points.Add(p);
                    break;
                }
                else
                    points.Add(p);
            }
            return points;
        }

        private List<Point> GetRookPoints(ChessPiece cur)
        {
            List<Point> points = new List<Point>();
            for (int i = 1; i < 8; i++)
            {
                Point p = new Point(cur.Pos.X - (i * 100), cur.Pos.Y);
                if (GetPieceByPoint(p) != null)
                {
                    points.Add(p);
                    break;
                }
                else
                    points.Add(p);
            }
            for (int i = 1; i < 8; i++)
            {
                Point p = new Point(cur.Pos.X + (i * 100), cur.Pos.Y);
                if (GetPieceByPoint(p) != null)
                {
                    points.Add(p);
                    break;
                }
                else
                    points.Add(p);
            }
            for (int i = 1; i < 8; i++)
            {
                Point p = new Point(cur.Pos.X, cur.Pos.Y + (i * 100));
                if (GetPieceByPoint(p) != null)
                {
                    points.Add(p);
                    break;
                }
                else
                    points.Add(p);
            }
            for (int i = 1; i < 8; i++)
            {
                Point p = new Point(cur.Pos.X, cur.Pos.Y - (i * 100));
                if (GetPieceByPoint(p) != null)
                {
                    points.Add(p);
                    break;
                }
                else
                    points.Add(p);
            }
            return points;
        }

        public bool IsPointValid(Point p, Player team)
        {
            bool valid = false;
            //IF THE POINT IS WITHIN THE CANVAS
            if (p.X >= 0 && p.X <= 700 && p.Y >= 0 && p.Y <= 700)
            {
                valid = true;
                ChessPiece cur = null;
                //GO THROUGH EACH PIECE AND CHECK IF A PIECE OF THAT SAME TEAM IS AT THE POINT p
                for (int i = 0; i < this.ChessBoard.Items.Count; i++)
                {
                    cur = (ChessPiece)this.ChessBoard.Items.GetItemAt(i);
                    if (cur.Type == PieceType.King && cur.Pos == p)
                        return true;
                    if (cur.Pos.X == p.X && cur.Pos.Y == p.Y && cur.Player == team)
                    {
                        valid = false;
                        break;
                    }
                }
            }
            return valid;
        }

        private List<Point> RemoveInvalidPoints(List<Point> list, Player team)
        {
            List<Point> newList = new List<Point>();
            foreach (Point p in list)
            {
                if (IsPointValid(p, team))
                    newList.Add(p);
            }
            return newList;
        }

        private void NextStage()
        {
            stage++;
            if (stage == 5)
                stage = 1;
        }

        private void PrevStage()
        {
            stage--;
            if (stage == 0)
                stage = 4;
        }

        TimeSpan endTime = new TimeSpan(0, 0, 0, 0, 0);
        TimeSpan interval = new TimeSpan(0, 0, 0, 0, 1000);
        private void BtnStartButton_Click(object sender, RoutedEventArgs e)
        {
            stage = 1;
            RbtnWhiteTurn.IsChecked = true;
            LblGameOver.Content = "";

            //HANDLES THE GAME RESTART
            if (BtnStartButton.Content.Equals("Restart"))
            {
                MessageBoxResult result = MessageBox.Show("Do you really want to restart?", "Restart Game", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        CanvasDots.Children.Clear();
                        break;
                    case MessageBoxResult.No:
                        return;
                }
                //PUT ALL THE PIECES BACK IN THEIR POSITIONS
                this.ChessBoard.ItemsSource = new ObservableCollection<ChessPiece>
            {
                new ChessPiece{Pos=new Point(0, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(100, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(200, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(300, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(400, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(500, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(600, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(700, 600), Type=PieceType.Pawn, Player=Player.White},
                new ChessPiece{Pos=new Point(0, 700), Type=PieceType.Rook, Player=Player.White},
                new ChessPiece{Pos=new Point(100, 700), Type=PieceType.Knight, Player=Player.White},
                new ChessPiece{Pos=new Point(200, 700), Type=PieceType.Bishop, Player=Player.White},
                new ChessPiece{Pos=new Point(400, 700), Type=PieceType.King, Player=Player.White},
                new ChessPiece{Pos=new Point(300, 700), Type=PieceType.Queen, Player=Player.White},
                new ChessPiece{Pos=new Point(500, 700), Type=PieceType.Bishop, Player=Player.White},
                new ChessPiece{Pos=new Point(600, 700), Type=PieceType.Knight, Player=Player.White},
                new ChessPiece{Pos=new Point(700, 700), Type=PieceType.Rook, Player=Player.White},
                new ChessPiece{Pos=new Point(0, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(100, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(200, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(300, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(400, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(500, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(600, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(700, 100), Type=PieceType.Pawn, Player=Player.Black},
                new ChessPiece{Pos=new Point(0, 0), Type=PieceType.Rook, Player=Player.Black},
                new ChessPiece{Pos=new Point(100, 0), Type=PieceType.Knight, Player=Player.Black},
                new ChessPiece{Pos=new Point(200, 0), Type=PieceType.Bishop, Player=Player.Black},
                new ChessPiece{Pos=new Point(400, 0), Type=PieceType.King, Player=Player.Black},
                new ChessPiece{Pos=new Point(300, 0), Type=PieceType.Queen, Player=Player.Black},
                new ChessPiece{Pos=new Point(500, 0), Type=PieceType.Bishop, Player=Player.Black},
                new ChessPiece{Pos=new Point(600, 0), Type=PieceType.Knight, Player=Player.Black},
                new ChessPiece{Pos=new Point(700, 0), Type=PieceType.Rook, Player=Player.Black},
            };
            }

            //start timer
            BtnStartButton.Content = "Restart";
            WhiteMinutes = new TimeSpan(0, 0, 10, 0, 0);
            BlackMinutes = new TimeSpan(0, 0, 10, 0, 0);
            t = new DispatcherTimer();
            t.Interval = interval;
            t.Tick += new EventHandler(White_Tick);
            t.Tick += new EventHandler(Black_Tick);
            TimeSpan ts = WhiteMinutes.Subtract(endTime);
            LblWhiteTime.Content = ts.ToString("m' : 's");
            LblBlackTime.Content = ts.ToString("m' : 's");
            t.Start();
        }

        private void White_Tick(object sender, EventArgs e)
        {
            if (WhiteMinutes > endTime && (stage == 1 || stage == 2))
            {
                WhiteMinutes = WhiteMinutes.Subtract(interval);
                LblWhiteTime.Content = WhiteMinutes.ToString("m' : 's");
            }
            else if (WhiteMinutes <= endTime) //when timer stops
            {
                t.Stop();
                LblWhiteTime.Content = "Time's up!";
                MessageBox.Show("You didn't finish in time.", "Sorry!");
                BtnStartButton.Content = "Restart";
                BtnStartButton.IsEnabled = true;
                stage = 6;
            }
        }

        private void Black_Tick(object sender, EventArgs e)
        {
            if (BlackMinutes > endTime && (stage == 3 || stage == 4))
            {
                BlackMinutes = BlackMinutes.Subtract(interval);
                LblBlackTime.Content = BlackMinutes.ToString("m' : 's");
            }
            else if (BlackMinutes <= endTime) //when timer stops
            {
                t.Stop();
                LblBlackTime.Content = "Time's up!";
                MessageBox.Show("You didn't finish in time.", "Sorry!");
                BtnStartButton.Content = "Restart";
                BtnStartButton.IsEnabled = true;
                stage = 6;
            }
        }

        private void EndGame()
        {
            String winner = "";
            if (stage == 1 || stage == 2)
                winner += "White";
            if (stage == 3 || stage == 4)
                winner += "Black";
            LblGameOver.Content = "Game Over: \n" + winner + " wins!";
            stage = 6;
            BtnStartButton.Content = "Restart";
            BtnStartButton.IsEnabled = true;
        }
    }
}
