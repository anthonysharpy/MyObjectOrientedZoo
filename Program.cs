using System;
using System.Collections.Generic;

/* Written by Anthony Sharp 2021 
 
 Features:

* Static classes
* Constructors
* Private members
* Properties
* Private setters
* Passing class instances as function arguments
* Class inheritance
* Lists
* Enums
* Structs

 */

namespace MyObjectOrientedZoo
{
    static class RandomNumbers
    {
        public static int GetRandomInt(int Max)
        {
            if (Max == 0) return 0;

            Random r = new Random(Guid.NewGuid().GetHashCode());
            return r.Next() % Max + 1;
        }

        public static float GetRandomFloat0to1()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            int n = r.Next() % 100000;
            return n / 100000.0f;
        }
    }

    static class StaticData
    {
        static string[] animalspecies = new string[] { "Tiger", "Giraffe", "Anteater", "Penguin", "Bee", "Lizard", "Elephant" };
        static string[] animaldiets = new string[] { "meat", "leaves", "insects", "fish", "nectar", "insects", "plants" };
        static string[] humannames = new string[] { "Donald", "Cameron", "Emma", "Kyleigh", "Beatrice", "Kaelyn", "Peter", "Suzie", "Richard", "Sarah", "Stacey", "Michael", "Tom", "Sophia", "Johnathan", "Lee", "Carl", "Olivia", "Sharon" };
        static string[] moodnames = new string[] { "Dead", "Sad", "Happy" };

        public static string GetAnimalMoodString(int mood)
        {
            return moodnames[mood];
        }

        public static string GetAnimalNameByID(int id)
        {
            return animalspecies[id];
        }

        public static string GetAnimalDietByID(int id)
        {
            return animaldiets[id];
        }

        public static int GetRandomSpeciesID()
        {
            return RandomNumbers.GetRandomInt(animalspecies.Length - 1);
        }

        public static char GetRandomCapitalLetter()
        {
            return (char)(RandomNumbers.GetRandomInt(25) + 65);
        }

        public static string GetRandomHumanName()
        {
            return humannames[RandomNumbers.GetRandomInt(humannames.Length - 1)] + " " + GetRandomCapitalLetter();
        }
    }

    public static class StringHelper
    {
        // I know this is technically wrong for words like "hour", but what can you do?
        public static string AorAn(string word)
        {
            if (word[0] == 'A'
                || word[0] == 'E'
                || word[0] == 'I'
                || word[0] == 'O'
                || word[0] == 'I') return "an";

            return "a";
        }
    }

    class LivingThing
    {
        public LivingThing()
        {
            chanceofgoingwrong = RandomNumbers.GetRandomFloat0to1();
            dailycost = RandomNumbers.GetRandomInt(10000) / 100.0f;
        }

        public int uniqueid;
        public float chanceofgoingwrong; // 0-1.
        public bool didsomethingwrongtoday;
        public float dailycost; // negative if earns money
    }

    class Animal:LivingThing
    {
        public Animal(int thespeciesid, int theuniqueid, Zoo thezoo)
        {
            speciesid = thespeciesid;
            uniqueid = theuniqueid;
            ourzoo = thezoo;
            dailycost = dailycost * -1;
        }

        public bool didnteattoday = false;
        private int mood = 2;
        public int speciesid;
        public Zoo ourzoo;

        public string Diet
        {
            get { return StaticData.GetAnimalDietByID(speciesid);  }
            set { }
        }

        public string Name
        {
            get
            {
                int nofspecies = ourzoo.NofSpecies(this);

                // If there are multiple species of this type at the zoo, attach a number at the end to indicate which one.
                if (nofspecies > 1) return StaticData.GetAnimalMoodString((int)mood) + StaticData.GetAnimalNameByID(speciesid)
                        + " (" + nofspecies + ")";
                else return StaticData.GetAnimalMoodString((int)mood) + StaticData.GetAnimalNameByID(speciesid); 
            }
            set { }
        }

        public int MoodInt
        {
            get { return mood; }
            set 
            {
                if (didnteattoday && value < mood) return; // Already didn't eat today, stop trying to starve them even more.
                if (value < mood) didnteattoday = true;

                mood = value;

                if(mood == 0)
                {
                    Console.WriteLine(Name + " starved to death!");
                    ourzoo.KillAnimal(this);
                }
                if (mood > 2) mood = 2;
            }
        }

        public void NewDay()
        {
            didnteattoday = false;
        }

        public void DoSomethingDumb()
        {
            string incidentdescription = "";
            didsomethingwrongtoday = true;
            Zoo.Incident.IncidentType type = 0;

            begin:
            int what = RandomNumbers.GetRandomInt(2);

            switch (what)
            {
                // Got eaten.
                case 0:
                    incidentdescription = Name + " snuck into the tiger enclosure and was eaten!";
                    ourzoo.KillAnimal(this);
                    type = Zoo.Incident.IncidentType.Died;
                    break;
                // Attacked a guest - lawsuit.
                case 1:
                    float money = RandomNumbers.GetRandomInt(10000) / 100.0f;
                    incidentdescription = Name + " attacked a guest. You were sued for " + money.ToString("c2") + "!";
                    ourzoo.bankbalance -= money;
                    Program.totalchange -= money;
                    type = Zoo.Incident.IncidentType.AttackedGuest;
                    break;
                // Got sick.
                case 2:
                    if (didnteattoday) goto begin; // We already didn't eat today. Doesn't matter if we are sick.
                    incidentdescription = Name + " got sick and didn't eat today!";
                    if (mood == 1) type = Zoo.Incident.IncidentType.Died;
                    else type = Zoo.Incident.IncidentType.GotSick;
                    MoodInt--;
                    break;
            }

            ourzoo.LogIncident(type, this, null, incidentdescription);
        }
    }

    class Staff:LivingThing
    {
        public string name;

        public Staff(int theuniqueid)
        {
            // Name.
            name = StaticData.GetRandomHumanName();
            uniqueid = theuniqueid;
        }

        public void DoSomethingDumb(Zoo thezoo)
        {
            didsomethingwrongtoday = true;
            string incidentdescription = "";
            Zoo.Incident.IncidentType type = 0;
            Animal animalinvoled = null;

            begin:
            int what = RandomNumbers.GetRandomInt(2);

            switch (what)
            {
                // Stole money.
                case 0:
                    float money = RandomNumbers.GetRandomInt(10000) / 100.0f;
                    incidentdescription = name + " stole " + money.ToString("c2") + " from your zoo!";
                    thezoo.bankbalance -= money;
                    Program.totalchange -= money;
                    type = Zoo.Incident.IncidentType.StoleMoney;
                    break;
                // Forgot to feed animal.
                case 1:
                    int animal = RandomNumbers.GetRandomInt(thezoo.animals.Count-1);
                    if (thezoo.animals[animal].didnteattoday) goto begin;
                    incidentdescription = name + " forgot to feed the " + thezoo.animals[animal].Name + "!";
                    thezoo.animals[animal].MoodInt--;
                    type = Zoo.Incident.IncidentType.ForgotFeeding;
                    animalinvoled = thezoo.animals[animal];
                    break;
                // Workplace-related incident.
                case 2:
                    incidentdescription = name + " had a workplace accident and was killed instantly!";
                    thezoo.KillStaff(this);
                    type = Zoo.Incident.IncidentType.Died;
                    break;
            }

            thezoo.LogIncident(type, animalinvoled, this, incidentdescription);
        }
    }

    class Zoo
    {
        public Zoo(string thename)
        {
            name = thename;
        }

        public string name;
        public float bankbalance = 1000;
        public List<Animal> animals = new List<Animal>();
        public List<Staff> staffmembers = new List<Staff>();
        public List<Incident> incidentstoday = new List<Incident>();

        public struct Incident
        {
            public Incident(string thedescription, IncidentType thetype, Animal theanimalinvolved, Staff thestaffinvolved)
            {
                description = thedescription;
                type = thetype;
                animalinvolved = theanimalinvolved;
                staffinvolved = thestaffinvolved;
            }

            public enum IncidentType
            {
                Died,
                ForgotFeeding,
                StoleMoney,
                AttackedGuest,
                GotSick
            }

            public IncidentType type;
            public Staff staffinvolved;
            public Animal animalinvolved;
            public string description;
        }

        public bool AnythingHappenedToday
        {
            get { return incidentstoday.Count > 0; }
            private set { }
        }

        public int NumberOfAnimals
        {
            get { return animals.Count; }
            private set { }
        }

        public int NumberOfStaff
        {
            get { return staffmembers.Count; }
            private set { }
        }

        public int NumberOfIncidentsToday
        {
            get { return incidentstoday.Count; }
            private set { }
        }

        public int NumberOfHungryAnimals
        {
            get
            {
                int n = 0;
                foreach(Animal a in animals) if (a.MoodInt < 2) n++;
                return n;
            }
            private set { }
        }

        public int NofSpecies(Animal a)
        {
            int n = 1;

            for(int i = 0; i < animals.Count; i++)
            {
                if (animals[i] == a) return n;
                else if (animals[i].speciesid == a.speciesid) n++;
            }

            return n;
        }

        public void KillAnimal(Animal a)
        {
            animals.Remove(a);
        }

        public void KillStaff(Staff s)
        {
            staffmembers.Remove(s);
        }

        public void GiveAnimals(int n)
        {
            for(int i = 0; i < n; i++)
            {
                int id = StaticData.GetRandomSpeciesID();
                Animal a = new Animal(id, animals.Count, this);
                animals.Add(a);
                Console.WriteLine("You adopted " + StringHelper.AorAn(animals[animals.Count - 1].Name) + " " + animals[animals.Count-1].Name + ", which eats " + animals[animals.Count-1].Diet);
            }
        }

        public void GiveStaff(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Staff s = new Staff(staffmembers.Count);
                staffmembers.Add(s);
                Console.WriteLine(staffmembers[staffmembers.Count-1].name + " was hired for " + (staffmembers[staffmembers.Count - 1].dailycost/8.0f).ToString("c2") + " per hour");
            }
        }

        public Animal GetAnimalByUniqueID(int id)
        {
            foreach (Animal a in animals)
            {
                if (a.uniqueid == id) return a;
            }

            return null;
        }

        public void ResetIncidents()
        {
            incidentstoday.Clear();

            foreach (Staff s in staffmembers) s.didsomethingwrongtoday = false;
            foreach (Animal a in animals) a.didsomethingwrongtoday = false;
        }

        public void LogIncident(Incident.IncidentType type, Animal animalinvolved, Staff staffinvolved, string description)
        {
            Console.WriteLine(description);
            Incident i = new Incident(description, type, animalinvolved, staffinvolved);
            incidentstoday.Add(i);
        }

        public Staff GetStaffByUniqueID(int id)
        {
            foreach (Staff s in staffmembers)
            {
                if (s.uniqueid == id) return s;
            }

            return null;
        }
    }

    class Program
    {
        static private int day = 1;
        static private Zoo ourzoo;
        public static float totalchange;

        static string GetZooName()
        {
            Console.WriteLine("Welcome to your new zoo! What would you like to call it");
            return Console.ReadLine();
        }

        static void SetupAnimals()
        {
            Console.WriteLine(ourzoo.name + "? That's a lovely name! And how many animals would " +
               "you like your zoo to have (it's probably best to start-off small)?");

            int numberofanimals;
            int.TryParse(Console.ReadLine(), out numberofanimals);

            while (numberofanimals < 1)
            {
                Console.WriteLine("Sorry I didn't quite catch that. Say again?");
                int.TryParse(Console.ReadLine(), out numberofanimals);
            }

            Console.WriteLine();
            ourzoo.GiveAnimals(numberofanimals);
        }

        static void SetupStaff()
        {
            Console.WriteLine("Great! Now, how many staff members would you like to hire?");

            int numberofstaff;
            int.TryParse(Console.ReadLine(), out numberofstaff);

            while (numberofstaff < 1)
            {
                Console.WriteLine("Sorry I didn't quite catch that. Say again?");
                int.TryParse(Console.ReadLine(), out numberofstaff);
            }

            Console.WriteLine();
            ourzoo.GiveStaff(numberofstaff);
        }

        struct PlayerChoice
        {
            public PlayerChoice(string desc, ChoiceType thetype, Zoo.Incident incident = new Zoo.Incident())
            {
                type = thetype;
                associatedincident = incident;
                description = desc;
            }

            public enum ChoiceType
            {
                Feed,
                PutDown,
                Fire,
                Nothing,
                GiveMedicine
            }

            public ChoiceType type;
            public Zoo.Incident associatedincident;
            public string description;
        }

        int HowManyChoices()
        {
            return ourzoo.NumberOfIncidentsToday + ourzoo.NumberOfHungryAnimals  + 1;
        }

        static List<PlayerChoice> GenerateChoices()
        {
            List<PlayerChoice> choices = new List<PlayerChoice>();

            int num = 1;
            // Include all incidents
            for (int i = 0; i < ourzoo.incidentstoday.Count; i++)
            {
                if (ourzoo.incidentstoday[i].type == Zoo.Incident.IncidentType.Died) continue; // They're dead - who cares?

                switch(ourzoo.incidentstoday[i].type)
                {
                    case Zoo.Incident.IncidentType.AttackedGuest:
                        choices.Add(new PlayerChoice(num + ") Put down " + ourzoo.incidentstoday[i].animalinvolved.Name,
                        PlayerChoice.ChoiceType.PutDown,
                        ourzoo.incidentstoday[i]));
                        break;
                    case Zoo.Incident.IncidentType.ForgotFeeding:
                        choices.Add(new PlayerChoice(num + ") Fire " + ourzoo.incidentstoday[i].staffinvolved.name,
                        PlayerChoice.ChoiceType.Fire,
                        ourzoo.incidentstoday[i]));
                        num++;
                        choices.Add(new PlayerChoice(num + ") Feed " + ourzoo.incidentstoday[i].animalinvolved.Name,
                        PlayerChoice.ChoiceType.Feed,
                        ourzoo.incidentstoday[i]));
                        break;
                    case Zoo.Incident.IncidentType.GotSick:
                        choices.Add(new PlayerChoice(num + ") Give medicine to " + ourzoo.incidentstoday[i].animalinvolved.Name,
                        PlayerChoice.ChoiceType.GiveMedicine,
                        ourzoo.incidentstoday[i]));
                        break;
                    case Zoo.Incident.IncidentType.StoleMoney:
                        choices.Add(new PlayerChoice(num + ") Fire " + ourzoo.incidentstoday[i].staffinvolved.name,
                        PlayerChoice.ChoiceType.Fire,
                        ourzoo.incidentstoday[i]));
                        break;
                }
                    
                num++;
            }

            choices.Add(new PlayerChoice(num + ") Do nothing", PlayerChoice.ChoiceType.Nothing));

            return choices;
        }

        static void PerformChoice(PlayerChoice pc)
        {
            Animal a;
            Staff s;

            switch(pc.type)
            {
                case PlayerChoice.ChoiceType.GiveMedicine:
                    a = pc.associatedincident.animalinvolved;
                    Console.WriteLine("You gave medicine to " + a.Name);
                    a.MoodInt++;
                    break;
                case PlayerChoice.ChoiceType.PutDown:
                    a = pc.associatedincident.animalinvolved;
                    Console.WriteLine("You put down " + a.Name);
                    ourzoo.KillAnimal(a);
                    break;
                case PlayerChoice.ChoiceType.Fire:
                    s = pc.associatedincident.staffinvolved;
                    Console.WriteLine("You fired " + s.name);
                    ourzoo.KillStaff(s);
                    break;
                case PlayerChoice.ChoiceType.Feed:
                    a = pc.associatedincident.animalinvolved;
                    Console.WriteLine("You fed " + a.Name);
                    a.MoodInt++;
                    break;
                case PlayerChoice.ChoiceType.Nothing:
                    Console.WriteLine("You did nothing today.");
                    break;
            }
        }
                             
        static void ResetAll()
        {
            foreach (Animal a in ourzoo.animals) a.NewDay();
            ourzoo.ResetIncidents();
        }

        static void CreateTrouble()
        {
            // See if staff do something dumb.
            for (int i = 0; i < ourzoo.staffmembers.Count; i++)
            {
                float n = RandomNumbers.GetRandomFloat0to1();

                if (n <= ourzoo.staffmembers[i].chanceofgoingwrong)
                {
                    ourzoo.staffmembers[i].DoSomethingDumb(ourzoo);
                }
            }

            // See if animals do something dumb.
            for (int i = 0; i < ourzoo.animals.Count; i++)
            {
                float n = RandomNumbers.GetRandomFloat0to1();

                if (n <= ourzoo.animals[i].chanceofgoingwrong)
                {
                    ourzoo.animals[i].DoSomethingDumb();
                }
            }
        }

        static void DescribeDay()
        {
            Console.WriteLine("INCOME:");
            foreach (Animal a in ourzoo.animals)
            {
                Console.WriteLine(a.Name + " brought in " + (-1 * a.dailycost).ToString("c2") + " in donations.");
                ourzoo.bankbalance -= a.dailycost;
                totalchange -= a.dailycost;
            }
            Console.WriteLine("\nOUTGOINGS:");
            foreach (Staff s in ourzoo.staffmembers)
            {
                Console.WriteLine(s.name + " took " + s.dailycost.ToString("c2") + " in wages.");
                ourzoo.bankbalance -= s.dailycost;
                totalchange -= s.dailycost;
            }
            Console.WriteLine("\nTODAY'S PROFIT: " + totalchange.ToString("c2"));
        }

        static int GetChoice(List<PlayerChoice> choices)
        {
            int choice;
            int.TryParse(Console.ReadLine(), out choice);
            while (choice < 1 || choice > choices.Count)
            {
                Console.WriteLine("Say again? You need to choose a number.");
                int.TryParse(Console.ReadLine(), out choice);
            }
            return choice;
        }

        static void DoGameLoop()
        {
            totalchange = 0.0f;

            Console.Clear();

            Console.WriteLine("Day " + day + " at " + ourzoo.name);
            Console.WriteLine("------------------------");
            Console.WriteLine("Bank balance: " + ourzoo.bankbalance.ToString("c2"));
            Console.WriteLine("Number of animals: " + ourzoo.NumberOfAnimals);
            Console.WriteLine("Number of staff: " + ourzoo.NumberOfStaff);
            Console.WriteLine("------------------------");

            // Reset what happened yesterday.
            ResetAll();

            // Create bad incidents.
            CreateTrouble();
            if (!ourzoo.AnythingHappenedToday) Console.WriteLine("Nothing happened today.");

            Console.WriteLine("------------------------");

            // Tell player what happened today.
            DescribeDay();

            Console.WriteLine("------------------------");

            // Give player choices.
            Console.WriteLine("What do you want to do today?");
            List<PlayerChoice> choices = GenerateChoices();

            // Print choices.
            foreach(PlayerChoice pc in choices) Console.WriteLine(pc.description);

            // Input choice.
            int choice = GetChoice(choices);

            Console.WriteLine("------------------------");

            PerformChoice(choices[choice - 1]);

            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();

            day++;
        }

        static void Main(string[] args)
        {
            ourzoo = new Zoo(GetZooName());
            Console.WriteLine();
            SetupAnimals();
            Console.WriteLine();
            SetupStaff();
            Console.WriteLine();

            // Explain game.
            Console.WriteLine("We're finally ready to open " + ourzoo.name + "! Every day you will " +
                "be tasked with running your zoo to the best of your ability. Be careful, because " +
                "things will go wrong, and since you are so busy, you will only have time to help " +
                "out in one way each day. Also, you're a very passive person, so you can only " +
                "react to sitatuions immediately after they happen. Good luck!\n\nPress any key to start.");
            Console.ReadKey();

            // Begin game loop.

            while(ourzoo.bankbalance > 0 && ourzoo.NumberOfAnimals > 0 && ourzoo.NumberOfStaff > 0)
            {
                DoGameLoop();
            }

            Console.Clear();
            Console.WriteLine("GAME OVER");
            if (ourzoo.NumberOfAnimals == 0) Console.WriteLine("You ran out of animals!");
            if (ourzoo.NumberOfStaff == 0) Console.WriteLine("You ran out of staff!");
            if (ourzoo.bankbalance == 0) Console.WriteLine("You ran out of money!");
            Console.WriteLine(ourzoo.name + " lasted " + day + " days.");
            Console.ReadKey();
        }
    }
}
