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

            q.Enqueue("start");
            q.Enqueue("next");
            q.Enqueue("end");
            Console.WriteLine("count=" + q.Count);

            Console.WriteLine(q.Peek());
            Console.WriteLine(q.Dequeue());
            Console.WriteLine(q.Dequeue());
            Console.WriteLine(q.Dequeue());
            Console.WriteLine("count=" + q.Count);

            try
            {
                q.Dequeue();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            q.Enqueue("newstart");
            Console.WriteLine("count=" + q.Count);
            Console.WriteLine(q.Dequeue());
            Console.WriteLine("count=" + q.Count);

            q.Enqueue("start");
            q.Enqueue("next");
            q.Enqueue("end");
            Console.WriteLine("count=" + q.Count);

            foreach (string s in q)
            {
                Console.WriteLine("Enumerate " + s);
            }

            Console.WriteLine("--------");

            PersistentQueue<string> pq = new PersistentQueue<string>(true);

            Console.WriteLine("count=" + pq.Count);

            pq.Enqueue("start");
            pq.Enqueue("next");
            pq.Enqueue("end");
            Console.WriteLine("count=" + pq.Count);

            Console.WriteLine(pq.Peek());
            Console.WriteLine(pq.Dequeue());
            Console.WriteLine(pq.Dequeue());
            Console.WriteLine(pq.Dequeue());
            Console.WriteLine("count=" + pq.Count);

            try
            {
                Console.WriteLine(pq.Dequeue());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            pq.Enqueue("newstart");
            Console.WriteLine("count=" + pq.Count);
            Console.WriteLine(pq.Dequeue());
            Console.WriteLine("count=" + pq.Count);

            pq.Enqueue("start");
            pq.Enqueue("next");
            pq.Enqueue("end");
            Console.WriteLine("count=" + pq.Count);

            foreach (string s in pq)
            {
                Console.WriteLine("Enumerate " + s);
            }

        }
    }
}
