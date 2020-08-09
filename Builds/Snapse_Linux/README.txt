Creating Neurons
Creating a neuron (via "New Neuron") will create an empty neuron with no rules and zero spikes. You can add rules and spikes using the "Edit Neuron" button.

You can also create an output neuron via "New Output" to receive the output of your system. This can serve as a substitute for the environment in SNP systems.

Editing Neurons
Clicking "Edit Neuron" toggles neuron editing mode where you can edit the spikes and rules of the neurons in the system. To add rules and spikes to an empty neuron, click the box below "Spikes" or "Rules", whichever you want to edit.

Clicking "Edit Neuron" again will exit neuron editing mode.

Writing Rules
To write a rule, follow the format "E/c->p;d" where E is a regular expression over {a} and c and p are strings over the alphabet {a} whose length is equal to the number of spikes to be consumed and to be produced, respectively. d, on the other hand, is a number equal to the delay. For example, a(aa)*/aaa→aa;2 is a valid rule while a(aa)*/a3→a2;2 and a(aa)*/a3→a2;2 are not. 

To write a forgetting rule simply leave p to be an empty string. For example, a*/a->;2 is a valid forgetting rule.

Deleting Neurons
Clicking "Delete Neuron" toggles neuron deletion mode. During neuron deletion mode, you can delete neurons by clicking them.

To exit neuron deletion mode, simply click "Delete Neuron" again.

Creating Synapses
To create synapses between two neurons, click "New Synapse" to toggle synapse creation mode. During synapse creation mode, the first neuron you click will be considered the source neuron and the next neuron you click will be the destination neuron, a synapse will then be created between the two. You can repeat the process with other neurons.

To exit the synapse creation mode simply click "New Synapse" again.

Deleting Synapses
Clicking "Delete Synapse" starts the synapse deletion mode. After clicking "Delete Synapse", round buttons with an X inside them will appear on all synapses and clicking it will the delete the synapse it's attached to.

If you're finished deleting synapses, simply click "Delete Synapse" again to exit the mode.

Step-by-step Simulation
You can simulate a system one step at a time by using the next step(>|) and the previous step(|<) buttons. 

The next step button simulates the system by one timestep while the previous step button reverts the system to its configuration in the previous timestep.

Continuous Simulation
You can simulate a system continuously. 

You can start continuous simulation using the play (>) button. During continuous simulation, the program simulates the system until the user clicks the pause (||) button or until the system halts.

Saving and Loading
The program autosaves new systems at "autosave.snapse". You can then change the save path by clicking "Save". After changing the path, the program will now autosave at the specified path.

For loading, the program looks at the "saves" folder in the application directory and lists the snapse files there. Note: The program doesn't look in subfolders.

About
Snapse is a graphical user interface for Spiking Neural P systems built with Unity3D and C# by Aleksei Fernandez and Reyster Fresco from the Algorithms and Complexity Lab (ACL) of the Department of Computer Science, University of the Philippines Diliman.

Snapse is built under the guidance of Francis Cabarle and with the help of colleagues in the ACL.
