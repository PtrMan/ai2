namespace PartialOrderedSet
{
    class PartialOrderedSetDagElement
    {

        public void reset()
        {
            referenceCounter = resetReferenceCounter;
            isRed = false;
            outputPosition = -1;
        }

        public int resetReferenceCounter = 0; // how many Dag Elements point at this element, is use to set "referenceCounter"
        public int referenceCounter = 0; // how many Da-Elements point at this element?
        public bool isRed = false; // used for marking mechanism
        public int outputPosition = -1;
    }
}
