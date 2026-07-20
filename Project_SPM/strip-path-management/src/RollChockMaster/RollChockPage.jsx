import { useEffect, useState } from "react";
import ChockMasterForm from "./components/ChockMasterForm";
import { fetchChockLookups } from "./services/chockLookupService";

export default function RollChockPage() {
  const [lookups, setLookups] = useState(null);

  useEffect(() => {
    fetchChockLookups().then(setLookups);
  }, []);

  if (!lookups) return <p>Loading...</p>;

  return <ChockMasterForm lookups={lookups} />;
}
