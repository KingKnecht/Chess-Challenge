using ChessChallenge.API;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //https://de.wikipedia.org/wiki/Alpha-Beta-Suche

            var root = new MyBot.Evaluation();

            var c1_1 = AddChild(root, 1);
            var c1_2 = AddChild(root, 1);
            var c1_3 = AddChild(root, 1);

            var c2_1 = AddChild(c1_1, 2);
            var c2_2 = AddChild(c1_1, 2);
            var c2_3 = AddChild(c1_2, 2);
            var c2_4 = AddChild(c1_2, 2);
            var c2_5 = AddChild(c1_3, 2);

            //Grandchildren
            var c3_1 = AddChild(c2_1, 3); c3_1.SetValue(10);
            var c3_2 = AddChild(c2_1, 3); c3_2.SetValue(-5);
            var c3_3 = AddChild(c2_1, 3); c3_3.SetValue(3);

            var c3_4 = AddChild(c2_2, 3); c3_4.SetValue(-6);
            var c3_5 = AddChild(c2_2, 3); c3_5.SetValue(12);
            var c3_6 = AddChild(c2_2, 3);

            var c3_7 = AddChild(c2_3, 3); c3_7.SetValue(10);
            var c3_8 = AddChild(c2_3, 3); c3_8.SetValue(12);
            var c3_9 = AddChild(c2_3, 3); c3_9.SetValue(3);

            var c3_10 = AddChild(c2_4, 3); c3_10.SetValue(13);
            var c3_11 = AddChild(c2_4, 3);
            var c3_12 = AddChild(c2_4, 3);

            var c3_13 = AddChild(c2_5, 3); c3_13.SetValue(3);
            var c3_14 = AddChild(c2_5, 3); c3_14.SetValue(2);
            var c3_15 = AddChild(c2_5, 3); c3_15.SetValue(-4);

            Assert.AreEqual(13, root.Value);

            Assert.AreEqual(-6, c1_1.Value);
            Assert.AreEqual(3, c1_2.Value);
            Assert.AreEqual(-4, c1_3.Value);


            Assert.AreEqual(10, c2_1.Value);
            Assert.AreEqual(12, c2_2.Value);
            Assert.AreEqual(12, c2_3.Value);
            Assert.AreEqual(13, c2_4.Value);
            Assert.AreEqual(3, c2_5.Value);
        }

        private MyBot.Evaluation AddChild(MyBot.Evaluation parent, int depth)
        {
            var child = new MyBot.Evaluation(Move.NullMove, parent, depth);
            parent.Children.Add(child);
            return child;

        }
    }
}