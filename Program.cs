using System;
using System.Collections.Generic;

namespace Queue
{
    class Program
    {
        static void Main(string[] args)
        {
            Queue<string> q = new Queue<string>();

            Console.WriteLine("count=" + q.Count);
            string s;
            s = "start";
            q.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            s = "next";
            q.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            s = "end";
            q.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            Console.WriteLine("count=" + q.Count);

            Console.WriteLine("Start=" + q.Peek());
            Console.WriteLine("Dequeue=" + q.Dequeue());
            Console.WriteLine("Dequeue=" + q.Dequeue());
            Console.WriteLine("Dequeue=" + q.Dequeue());
            Console.WriteLine("count=" + q.Count);

            try
            {
                Console.WriteLine(q.Dequeue());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            s = "newstart";
            q.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            Console.WriteLine("count=" + q.Count);
            Console.WriteLine("Dequeue=" + q.Dequeue());
            Console.WriteLine("count=" + q.Count);

            s = "start";
            q.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            s = "next";
            q.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            s = "end";
            q.Enqueue(s);
            Console.WriteLine("count=" + q.Count);

            foreach (string st in q)
            {
                Console.WriteLine("Enumerate=" + st);
            }

            Console.WriteLine("--------");

            PersistentQueue<string> pq = new PersistentQueue<string>(true);

            Console.WriteLine("count=" + pq.Count);
            s = "start";
            pq.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            s = "next";
            pq.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            s = "end";
            pq.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            Console.WriteLine("count=" + pq.Count);

            Console.WriteLine("Start=" + pq.Peek());
            Console.WriteLine("Dequeue=" + pq.Dequeue());
            Console.WriteLine("Dequeue=" + pq.Dequeue());
            Console.WriteLine("Dequeue=" + pq.Dequeue());
            Console.WriteLine("count=" + pq.Count);

            try
            {
                Console.WriteLine(pq.Dequeue());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            s = "newstart";
            pq.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            Console.WriteLine("count=" + pq.Count);
            Console.WriteLine("Dequeue=" + pq.Dequeue());
            Console.WriteLine("count=" + pq.Count);

            s = "start";
            pq.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            s = "next";
            pq.Enqueue(s);
            Console.WriteLine("Enqueue=" + s);
            s = "end";
            pq.Enqueue(s);
            Console.WriteLine("count=" + pq.Count);

            foreach (string st in pq)
            {
                Console.WriteLine("Enumerate=" + st);
            }

        }
    }
}
