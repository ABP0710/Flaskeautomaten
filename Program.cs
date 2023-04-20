using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;

namespace Flaskeautomaten
{
    internal class Program
    {

        //buffer for the bottel
        static Queue<Bottel> mixtedCase = new Queue<Bottel>();

        //buffer for the beerbottels
        static Queue<Bottel> caseOfBeer = new Queue<Bottel>();

        //buffer for the sodabottels
        static Queue<Bottel> caseOfSoda = new Queue<Bottel>();

        static void Main(string[] args)
        {
            //4 threads 
            Thread p = new Thread(Producer);
            p.Name = "Producer";
            p.Start();

            Thread s = new Thread(() => Splitter());
            s.Name = "Splitter";
            s.Start();

            Thread b = new Thread(() => GetBeer());
            b.Name = "Beer";
            b.Start();

            Thread w = new Thread(() => GetSoda());
            w.Name = "Water";
            w.Start();

            p.Join();
            s.Join();
            b.Join();
            w.Join();
        }

        public static void Producer()
        {
            //random used for both the forloop to determen the number of bottels there is made, and to set the type on the bottels to make it beer or soda
            Random rdm = new Random();

            
            while (true)
            {
                //lock on the mixtedCase queue
                lock (mixtedCase)
                //enter the resurce on the { of lock
                {
                    try
                    {
                        //generate a random number of the bottel objects, sets the type and put the bottel in the mixtedCase queue
                        for (int i = 0; i < rdm.Next(1, 5); i++)
                        {
                            Bottel bottel = new Bottel(rdm.Next(1, 3));
                            mixtedCase.Enqueue(bottel);
                        }

                        Console.WriteLine("Ny flaske");
                        Thread.Sleep(500);
                        //informs that there is change to the resurce
                        Monitor.PulseAll(mixtedCase);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Fejl " + e);
                    }
                //exit the resurce at the } of lock
                }
            }
        }

        public static Bottel Splitter()
        {
            while (true)
            {
                //enter mixtedCase
                Monitor.Enter(mixtedCase);

                try
                {
                    //wait until the queue is is't empty
                    if (mixtedCase.Count == 0)
                    {
                        Console.WriteLine("Sortering: Flaskebånd klar til modtagelse!");
                        Thread.Sleep(500);
                        Monitor.Wait(mixtedCase);
                    }

                    //when the bottel is of type 1 
                    if (mixtedCase.Peek().type.Equals(1))
                    {
                        //enter caseOfBeer queue
                        Monitor.Enter(caseOfBeer);

                        try
                        {
                            //the bottel is now a beer and is taken out of the mixtedCase queue
                            Bottel beer = mixtedCase.Dequeue();
                            //the beer is then added to the caseOfBeer queue
                            caseOfBeer.Enqueue(beer);
                            Console.WriteLine("Sortering: Ølflaske klar til afhentning");
                            Thread.Sleep(250);
                            //informs that there is change to the resurce
                            Monitor.PulseAll(caseOfBeer);
                        }
                        finally
                        {
                            //exit the caseOfBeer resurce
                            Monitor.Exit(caseOfBeer);
                        }
                    }
                    //when the bottel is of type 2
                    else if (mixtedCase.Peek().type.Equals(2))
                    {
                        //enter the caseOfSoda queue
                        Monitor.Enter(caseOfSoda);

                        try
                        {
                            //the bottel is now a soda an is taken out of the mixtedCase queue
                            Bottel soda = mixtedCase.Dequeue();
                            //the soda is added to the caseOfSoda queue
                            caseOfSoda.Enqueue(soda);
                            Console.WriteLine("Sortering: Sodavands flaske klar til afhentning");
                            Thread.Sleep(250);
                            //informs that there is change to the resurce
                            Monitor.PulseAll(caseOfSoda);
                        }
                        finally
                        {
                            //exit the caseOfSoda resurce
                            Monitor.Exit(caseOfSoda);
                        }
                    }
                }
                finally
                {
                    Console.WriteLine("Sortering: Flaske modtaget");
                    Thread.Sleep(250);
                    //exit the mixtedCase resurce
                    Monitor.Exit(mixtedCase);
                }
            }
        }

        public static Bottel GetBeer()
        {
            while (true)
            {
                //lock on the caseOfBeer queue
                lock (caseOfBeer)
                //enter the caseOfBeer resurce
                {
                    try
                    {
                        //when the queue is empty wait
                        while (caseOfBeer.Count == 0)
                        {
                            Console.WriteLine("Ølvogn klar til retur");
                            Thread.Sleep(250);
                            Monitor.Wait(caseOfBeer);
                        }
                    }
                    finally
                    {
                        Console.WriteLine("Ølvogn modtager!");
                        Thread.Sleep(100);
                        //the beer is removed fromthe caseOfBeer queue
                        caseOfBeer.Dequeue();
                    }
                //exit the caseOfBeer
                }
            }
        }

        public static Bottel GetSoda()
        {
            while (true)
            {
                //lock on the caseOfSoda
                lock (caseOfSoda)
                //enter the caseOfSoda resurce
                {
                    try
                    {
                        //when the queue is empty wait
                        while (caseOfSoda.Count == 0)
                        {
                            Console.WriteLine("Vandvognen klar til retur");
                            Thread.Sleep(250);
                            Monitor.Wait(caseOfSoda);
                        }
                    }
                    finally
                    {
                        Console.WriteLine("Vandvogn modtager!");
                        Thread.Sleep(100);
                        //the soda is removed from the queue
                        caseOfSoda.Dequeue();
                    }
                //exit the caseOfSoda
                }
            }
        }
    }

    //bottel class with an int named type, where is set when the bottel obj is created
    public class Bottel
    {
        public int type { get; set; }

        public Bottel(int type)
        {
            this.type = type;
        }
    }
}