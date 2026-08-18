from pydispatch import dispatcher

UNKNOWN = "unk"

def handle_unknown( sender ):
    """Simple event handler"""
    print(f"Unknown signal by '{sender}'")
dispatcher.connect( handle_unknown, signal=UNKNOWN, sender=dispatcher.Any )
print("Testing unknown signal")
dispatcher.send( signal=UNKNOWN, sender=None )
dispatcher.send( signal="unk", sender="Something" )
print("")

TWEET = 'tweet'
class Bird:
    def talk(self):
        dispatcher.send( signal=TWEET, sender=self )
robin = Bird()

def girl_robin( ):
    print(f"Robin: Tweet!")
dispatcher.connect( girl_robin, signal=TWEET, sender=robin ) #only listens to robin

def birdwatcher( ):
    print(f"I heard a Tweet!")
dispatcher.connect( birdwatcher, signal=TWEET, sender=dispatcher.Anonymous ) #only listens to anonymous

print("Testing sender restrictions")
dispatcher.send( signal=TWEET, sender=None ) #not received
print("<tweet not received>")
dispatcher.send( signal=TWEET, sender=dispatcher.Anonymous ) #only by birdwatcher
robin.talk() #only by girl_robin
print("")

print("Testing with required parameters")
SOUND = "sound"
def handle_sound( sender, sound=None): #allows non-definition
    print(f"{sender}: {sound}!")
dispatcher.connect( handle_sound, signal=SOUND, sender=dispatcher.Any )

dispatcher.send(signal=SOUND, sender="Cow", sound="Moo") #parameter name must match-
dispatcher.send(signal=SOUND, sender="Rabbit")

