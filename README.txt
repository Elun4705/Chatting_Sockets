Author:     Andy Huo
Partner:    Emmanuel Luna
Date:       3/28/2023
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  ahuo2003, Elun4705
Repo:       https://github.com/uofu-cs3500-spring23/assignment-seven---chatting-andyluna
Date:       4/6/23
Solution:   Networking_and_Logging
Copyright:  CS 3500, Andy Huo & Emmanuel Luna - This work may not be copied for use in Academic Coursework.
```

# Overview of the Chatting functionality:
    The server is currently able to keep track of indidivudal clients and propagate recieved messages among all of them.

    - Note: This is based off of theory and a brief overview by a TA.  We haven't been able to properly test this to its full extent as a result of exenuating circumstances.

# Time Expenditures:

    1. ChatClient                                 Predicted Hours:    5        Actual Hours:   6
    2. ChatServer                                 Predicted Hours:    5        Actual Hours:   8 
    3. Communications                             Predicted Hours:    7        Actual Hours:   18
    4. FileLogger                                 Predicted Hours:    4        Actual Hours:   3

    5. Networking_and_Logging                     Predicted Hours:    15       Actual Hours:   35
    


# Examples of Good Software Practice:
    1. We made use of several simple helper methods to take care of a lot of the heavy lifting in addition to a FileLogger class implemented by Andy (during lab).

# On time management and estimation skills:
    Our ability to manage and predict out workload was horribly of scale, what we expected to take only a usual week took us two weeks to figure out
    and gain a grasp on our knowledge. Unlike our spreadsheet where we said we we have thought out time prediction might improve, we prove ourselves wrong.
    Hopefully for the next time we can try to work our actual time limits to work on and meet smaller deadlines that can help guide us or at least prompt us
    to do more or ask for more help.

# Design Decisions:
    1. Our design may end up looking rather simple, as we spent most of our time trying to get the internals to work first.  It should still be fairly intuitive, though.
    We did ultiamtely make the decision to force people to input a username before they connected, in order to prevent people from not having an identifiable name.
    2. We decided to make our logger log at an "information" level.  Our justification for this is that this is more-or-less intended for use as a final product, and thus
    any logging we do should be used at a level for maintaining the server.  By the definitions, "Information" seemed like the best bet.

# Problems:
    Our biggest problem with the code was actually understanding what it wanted us to do.  From the sounds of it, so many things were sort of coming at us all at once,
    leaving us kind of confused and unable to keep up.  Catching up and getting to the point where we felt comfortable working with the tools we were given took a while,
    and by that point we had already spent a lot of time on our code.

# External Resources:
    Most of the questions we had were answered by Piazza.

# Implementation Notes:
    Closing a server should disconnect every user (again, we had difficulties in testing multiple clients)
    Closing a client should disconnect them from the server
    You should not be able to request participants, or send a message without joining a server.  You should not be able to join a server without having a set name first

# Partnership:
    Most of the code was done together, with the exception of a few parts which we did seperately (namely the Filelogger, as Andy had already implemented it in lab)\

# Branching:
    There was no branching

# Testing:
    Most of our testing was done via the usage of simply opening up the GUI and checking for any errors/unexpected features in the way we manipulated it


