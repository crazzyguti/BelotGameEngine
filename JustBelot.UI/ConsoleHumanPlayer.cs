﻿namespace JustBelot.UI
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using JustBelot.Common;

    public class ConsoleHumanPlayer : IPlayer
    {
        private readonly Hand cards;

        public ConsoleHumanPlayer(string name)
        {
            this.Name = name;
            this.cards = new Hand();
        }

        public string Name { get; private set; }

        private GameInfo Game { get; set; }

        private PlayerPosition Position { get; set; }

        private Contract Contract { get; set; }

        public void StartNewGame(GameInfo game, PlayerPosition position)
        {
            Console.Clear();
            this.Position = position;
            this.Game = game;
            this.Game.PlayerBid += this.GamePlayerBid;
        }

        public void StartNewDeal()
        {
            this.cards.Clear();
            this.Contract = new Contract();
            this.Draw();
        }

        public void AddCard(Card card)
        {
            this.cards.Add(card);
            this.Draw();
        }

        public BidType AskForBid(Contract currentContract, IList<BidType> availableBids, IList<BidType> previousBids)
        {
            this.Contract = currentContract;
            while (true)
            {
                this.Draw();

                var availableBidsAsString = new StringBuilder();
                foreach (var availableBid in availableBids)
                {
                    switch (availableBid)
                    {
                        case BidType.Pass:
                            availableBidsAsString.Append("P(ass)");
                            break;
                        case BidType.Clubs:
                            availableBidsAsString.Append("C(♣)");
                            break;
                        case BidType.Diamonds:
                            availableBidsAsString.Append("D(♦)");
                            break;
                        case BidType.Hearts:
                            availableBidsAsString.Append("H(♥)");
                            break;
                        case BidType.Spades:
                            availableBidsAsString.Append("S(♠)");
                            break;
                        case BidType.NoTrumps:
                            availableBidsAsString.Append("N(o)");
                            break;
                        case BidType.AllTrumps:
                            availableBidsAsString.Append("A(ll)");
                            break;
                        case BidType.Double:
                            availableBidsAsString.Append("2(double)");
                            break;
                        case BidType.ReDouble:
                            availableBidsAsString.Append("4(re double)");
                            break;
                    }

                    availableBidsAsString.Append(", ");
                }

                ConsoleHelper.WriteOnPosition(availableBidsAsString.ToString().Trim(' ', ','), 0, 19);
                ConsoleHelper.WriteOnPosition("It's your turn! Please enter your bid: ", 0, 18);

                var bid = BidType.Pass;

                var playerContract = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(playerContract))
                {
                    continue;
                }

                switch (char.ToUpper(playerContract[0]))
                {
                    case 'A':
                        bid = BidType.AllTrumps;
                        break;
                    case 'N':
                        bid = BidType.NoTrumps;
                        break;
                    case 'S':
                        bid = BidType.Spades;
                        break;
                    case 'H':
                        bid = BidType.Hearts;
                        break;
                    case 'D':
                        bid = BidType.Diamonds;
                        break;
                    case 'C':
                        bid = BidType.Clubs;
                        break;
                    case 'P':
                        bid = BidType.Pass;
                        break;
                    case '2':
                        bid = BidType.Double;
                        break;
                    case '4':
                        bid = BidType.ReDouble;
                        break;
                    default:
                        continue;
                }

                if (availableBids.Contains(bid))
                {
                    return bid;
                }
            }
        }

        public IEnumerable<Declaration> AskForDeclarations()
        {
            // TODO: Find declarations and ask user for them
            ConsoleHelper.WriteOnPosition("No declarations available.", 0, 18);
            return new List<Declaration>();
        }

        public PlayAction PlayCard()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < this.cards.Count; i++)
            {
                sb.AppendFormat("{0}({1}); ", i + 1, this.cards[i]);
            }

            while (true)
            {
                this.Draw();
                ConsoleHelper.WriteOnPosition(sb.ToString().Trim(), 0, 19);
                ConsoleHelper.WriteOnPosition("It's your turn! Please select card to play: ", 0, 18);
                var cardIndexAsString = Console.ReadLine();
                int cardIndex;
                if (int.TryParse(cardIndexAsString, out cardIndex))
                {
                    if (cardIndex > 0 && cardIndex <= this.cards.Count)
                    {
                        // TODO: Ask player if he wants to announce belote
                        var cardToPlay = this.cards[cardIndex - 1];
                        this.cards.RemoveAt(cardIndex - 1);
                        return new PlayAction(cardToPlay);
                    }
                }
            }
        }

        private void GamePlayerBid(BidEventArgs bidEventArgs)
        {
            this.Contract = bidEventArgs.CurrentContract;
            this.Draw();
            if (bidEventArgs.Position != this.Position)
            {
                if (bidEventArgs.Position == PlayerPosition.East)
                {
                    ConsoleHelper.DrawTextBoxTopRight(bidEventArgs.Bid.ToString(), 80 - 2 - this.Game[PlayerPosition.East].Name.Length - 2, 7);
                }

                if (bidEventArgs.Position == PlayerPosition.North)
                {
                    ConsoleHelper.DrawTextBoxTopLeft(bidEventArgs.Bid.ToString(), 40 - 2 - (bidEventArgs.Bid.ToString().Length / 2), 2);
                }

                if (bidEventArgs.Position == PlayerPosition.West)
                {
                    ConsoleHelper.DrawTextBoxTopLeft(bidEventArgs.Bid.ToString(), this.Game[PlayerPosition.West].Name.Length + 3, 7);
                }

                ConsoleHelper.WriteOnPosition(string.Format("{0} from {1} player", bidEventArgs.Bid, bidEventArgs.Position), 0, 18);
                ConsoleHelper.WriteOnPosition("Press enter to continue...", 0, 19);
                Console.ReadLine();
            }
        }

        private void Draw()
        {
            ConsoleHelper.ClearAndResetConsole();
            ConsoleHelper.DrawTextBoxTopRight(string.Format("{0} - {1}", this.Game.SouthNorthScore, this.Game.EastWestScore), Console.WindowWidth - 1, 0, ConsoleColor.Black, ConsoleColor.DarkGray);
            ConsoleHelper.DrawTextBoxTopLeft(this.Contract.ToString(), 0, 0, ConsoleColor.Black, ConsoleColor.DarkGray);
            ConsoleHelper.WriteOnPosition(this.Game[PlayerPosition.West].Name, 2, 8, ConsoleColor.Black, ConsoleColor.Gray);
            ConsoleHelper.WriteOnPosition(this.Game[PlayerPosition.East].Name, 80 - 2 - this.Game[PlayerPosition.East].Name.Length, 8, ConsoleColor.Black, ConsoleColor.Gray);
            ConsoleHelper.WriteOnPosition(this.Game[PlayerPosition.North].Name, 40 - 1 - (this.Game[PlayerPosition.North].Name.Length / 2), 1, ConsoleColor.Black, ConsoleColor.Gray);
            ConsoleHelper.WriteOnPosition(this.Game[PlayerPosition.South].Name, 40 - 1 - (this.Game[PlayerPosition.South].Name.Length / 2), 17, ConsoleColor.Black, ConsoleColor.Gray);

            int left = 40 - 1 - (this.cards.ToString().Replace(" ", string.Empty).Length / 2);
            this.cards.Sort(ContractType.AllTrumps);
            foreach (var card in this.cards)
            {
                var cardAsString = card.ToString();
                ConsoleColor color;
                if (card.Suit == CardSuit.Diamonds || card.Suit == CardSuit.Hearts)
                {
                    color = ConsoleColor.Red;
                }
                else
                {
                    color = ConsoleColor.Black;
                }

                ConsoleHelper.WriteOnPosition(cardAsString, left, 16, color, ConsoleColor.White);
                left += cardAsString.Length;
            }
        }
    }
}