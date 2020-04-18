using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceDetectionAndRecognition.Database
{
    public class EntityContext : DbContext
    {
        public EntityContext() : base("")
        {

        }
    }
}
